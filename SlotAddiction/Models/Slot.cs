using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using Reactive.Bindings;
using SlotAddiction.DataBase;
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
        /// 店舗情報
        /// </summary>
        private readonly DbSet<Tempo> _tempos = new SlotAddictionDBContext().Tempos;
        /// <summary>
        /// フロアのURL
        /// </summary>
        private Url _floorUrl;
        /// <summary>
        /// 遊技台のURL
        /// </summary>
        private Url _slotDataUrl;
        #endregion

        #region プロパティ
        /// <summary>
        /// 遊技履歴を格納
        /// </summary>
        public ReactiveCollection<SlotPlayData> SlotPlayDataCollection { get; set; } = new ReactiveCollection<SlotPlayData>();
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
        /// <param name="slotModels"></param>
        /// <returns></returns>
        public async Task GetSlotDataAsync(DateTime dataDate, List<string> slotModels)
        {
            try
            {
                foreach (var tempo in _tempos)
                {

                    var slotMachineStartNo = tempo.SlotMachineStartNo;
                    var slotMachineEndNo = tempo.SlotMachineEndNo;
                    var slotMachineNumbers = Enumerable.Range(slotMachineStartNo, slotMachineEndNo - slotMachineStartNo + 1);

                    if (slotModels != null)
                    {
                        foreach (var slotModel in slotModels)
                        {
                            //店舗のフロアURLを作成
                            _floorUrl = new Url($"{tempo.StoreURL}unit_list?model={slotModel}");

                            //URL内のソースを取得
                            try
                            {
                                var response = await _httpClient.GetStreamAsync(_floorUrl);

                                //取得したソースを解析
                                //後で変数名を変更しよう
                                var floorDataForSlotModel = await _analysisHTML.AnalyseFloorAsync(response, slotModels);
                                slotMachineNumbers = floorDataForSlotModel;
                            }
                            catch
                            {
                                //指定した機種が該当のホールになかったと判定する
                                Console.WriteLine($"{tempo.StoreName}に{slotModel}はありませんでした。");
                                slotMachineNumbers = new List<int>();
                            }
                        }
                    }

                    foreach (var slotMachinNumber in slotMachineNumbers)
                    {
                        var month = $"{dataDate.Month:D2}";
                        var day = $"{dataDate.Day:D2}";

                        //リクエストを投げるURLを作成
                        _slotDataUrl = new Url($"{tempo.StoreURL}detail?unit={slotMachinNumber}&target_date={dataDate.Year}-{month}-{day}");

                        //URL内のソースを取得
                        var response = await _httpClient.GetStreamAsync(_slotDataUrl);

                        //取得したソースを解析
                        var analysisSlotData = await _analysisHTML.AnalyseAsync(response, tempo, slotModels);

                        //解析したデータをコレクションに追加
                        SlotPlayDataCollection.Add(analysisSlotData);
                    }
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
                //throw;
            }
        }
        #endregion
    }
}
