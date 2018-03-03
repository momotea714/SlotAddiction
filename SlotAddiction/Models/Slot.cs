using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using Reactive.Bindings;
using SlotAddiction.DataBase;
using SlotAddiction.Extentions;
using SlotAddiction.Models;

namespace SlotData.Models
{
    public class Slot
    {
        #region フィールド
        /// <summary>
        /// usingステートメントを使用して何度もソケットを開放するとリソースを食いつぶす為staticなHttpClientを作成する
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient();
        /// <summary>
        /// HTML解析オブジェクト
        /// </summary>
        private readonly AnalysisHTML _analysisHTML = new AnalysisHTML();
        /// <summary>
        /// DB
        /// </summary>
        private readonly SlotAddictionDBContext _db = new SlotAddictionDBContext();
        /// <summary>
        /// 店舗情報
        /// </summary>
        private readonly DbSet<Tempo> _tempos;
        /// <summary>
        /// 機種情報
        /// </summary>
        private readonly DbSet<SlotModel> _slotModels;
        /// <summary>
        /// フロアのURL
        /// </summary>
        private readonly HashSet<Url> _floorUrl = new HashSet<Url>();
        /// <summary>
        /// 遊技台のURL
        /// </summary>
        private readonly HashSet<Url> _slotDataUrl = new HashSet<Url>();
        #endregion

        #region プロパティ
        /// <summary>
        /// 遊技履歴を格納
        /// </summary>
        public ReactiveCollection<SlotPlayData> SlotPlayDataCollection { get; set; } = new ReactiveCollection<SlotPlayData>();
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 
        /// </summary>
        public Slot()
        {
            _tempos = _db.Tempos;
            _slotModels = _db.SlotModels;
        }
        #endregion

        #region メソッド
        /// <summary>
        /// 遊技台のデータ取得
        /// </summary>
        /// <param name="dataDate">データを取得したい日付</param>
        public async Task GetSlotDataAsync(DateTime dataDate)
        {
            await GetSlotDataAsync(dataDate, null);
        }
        /// <summary>
        /// 遊技台のデータ取得
        /// </summary>
        /// <param name="dataDate"></param>
        /// <param name="inputSlotModels"></param>
        /// <returns></returns>
        public async Task GetSlotDataAsync(DateTime dataDate, List<string> inputSlotModels)
        {
            foreach (var tempo in _tempos)
            {
                //初期化
                _floorUrl.Clear();
                _slotDataUrl.Clear();

                var slotMachineStartNo = tempo.SlotMachineStartNo;
                var slotMachineEndNo = tempo.SlotMachineEndNo;
                var slotMachineNumbers = Enumerable.Range(slotMachineStartNo, slotMachineEndNo - slotMachineStartNo + 1);

                if (inputSlotModels != null)
                {
                    //指定した台が店舗に存在するか確かめられるURLを作成
                    _floorUrl.AddRange(inputSlotModels.Select(slotModel => new Url($"{tempo.StoreURL}unit_list?model={slotModel}")));

                    try
                    {
                        var floorStreamTasks = _floorUrl.Select(floorUrl => _httpClient.GetStreamAsync(floorUrl));
                        var streamResponses = await Task.WhenAll(floorStreamTasks);

                        //取得したソースを解析
                        var floorAnalyseTasks = streamResponses.Select(response => _analysisHTML.AnalyseFloorAsync(response, inputSlotModels));
                        var slotMachineNumbersForSlotModels = await Task.WhenAll(floorAnalyseTasks);

                        //リストの平準化
                        slotMachineNumbers = slotMachineNumbersForSlotModels.SelectMany(x => x);
                    }
                    catch
                    {
                        //指定した機種が該当のホールになかったと判定する
                        //Console.WriteLine($"{tempo.StoreName}に{slotModel}はありませんでした。");
                        slotMachineNumbers = new HashSet<int>();
                    }
                }

                try
                {
                    //遊技台の情報があるURLを作成する
                    var month = $"{dataDate.Month:D2}";
                    var day = $"{dataDate.Day:D2}";
                    _slotDataUrl.AddRange(slotMachineNumbers.Select(slotMachineNumber => new Url($"{tempo.StoreURL}detail?unit={slotMachineNumber}&target_date={dataDate.Year}-{month}-{day}")));

                    //URL内のソースを取得
                    var slotDataStreamTasks = _slotDataUrl.Select(url => _httpClient.GetStreamAsync(url));
                    var streamResponses  = await Task.WhenAll(slotDataStreamTasks);

                    //取得したソースを解析
                    var slotDataAnalyseTasks = streamResponses.Select(response => _analysisHTML.AnalyseAsync(response, _slotModels, tempo, inputSlotModels));
                    var analysisSlotData = await Task.WhenAll(slotDataAnalyseTasks);

                    //解析したデータをコレクションに追加
                    foreach (var slotPlayData in analysisSlotData)
                    {
                        SlotPlayDataCollection.Add(slotPlayData);
                    }
                }
                catch (Exception e)
                {
                    if (e == new HttpRequestException())
                    {
                        var check = _floorUrl;
                        var check2 = _slotDataUrl;
                        //指定されたURLが不正です。
                    }
                    throw;
                }
            }
        }
        #endregion
    }
}