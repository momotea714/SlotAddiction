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
        /// <param name="slotModel"></param>
        /// <returns></returns>
        public async Task GetSlotDataAsync(DateTime dataDate, List<string> slotModel)
        {
            try
            {
                foreach (var tempo in _tempos)
                {
                    var slotMachineStartNo = tempo.SlotMachineStartNo;
                    var slotMachineEndNo = tempo.SlotMachineEndNo;
                    var slotMachineNumbers = Enumerable.Range(slotMachineStartNo, slotMachineEndNo - slotMachineStartNo + 1);

                    if (slotModel != null)
                    {
                        //店舗のフロアURLを作成
                        var floorUrl = new Url($"{tempo.StoreURL}floor");

                        //URL内のソースを取得
                        var response = await _httpClient.GetStreamAsync(floorUrl);

                        //取得したソースを解析
                        //後で変数名を変更しよう
                        var floorDataForSlotModel = await _analysisHTML.AnalyseFloorAsync(response, slotModel);

                        if (floorDataForSlotModel.Any())
                        {
                            //指定した機種が存在する台番号だけが格納されているリストに差し替える
                            slotMachineNumbers = floorDataForSlotModel;
                        }
                        else
                        {
                            //指定した機種が該当のホールになかったと判定する
                            return;
                        }
                    }

                    foreach(var slotMachinNumber in slotMachineNumbers)
                    {
                        //リクエストを投げるURLを作成
                        var slotDataUrl = new Url($"{tempo.StoreURL}detail?unit={slotMachinNumber}&target_date={dataDate.Year}-{dataDate.Month}-{dataDate.Day}");

                        //URL内のソースを取得
                        var response = await _httpClient.GetStreamAsync(slotDataUrl);

                        //取得したソースを解析
                        var analysisSlotData = await _analysisHTML.AnalyseAsync(response, slotModel);

                        //解析したデータをコレクションに追加
                        SlotPlayDataCollection.Add(analysisSlotData);
                    }
                }
            }
            catch (Exception e)
            {
                if (e == new HttpRequestException())
                {
                    //指定されたURLが不正です。
                }
                throw;
            }
        }
        #endregion
    }
}
