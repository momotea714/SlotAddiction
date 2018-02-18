using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SlotAddiction.DataBase;
using SlotAddiction.Models;

namespace SlotData.Models
{
    public class Slot
    {
        #region フィールド

        private Url _slotDataApiUrl;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly AnalysisHTML _analysisHTML = new AnalysisHTML();
        #endregion

        #region プロパティ
        public ReactiveCollection<SlotPlayData> SlotPlayDataCollection { get; set; } = new ReactiveCollection<SlotPlayData>();
        #endregion

        #region コンストラクタ
        public Slot()
        {
            //_analysisHTML.SlotPlayData.PropertyChangedAsObservable().Subscribe(_ =>
            //    SlotPlayDataCollection.Add(_analysisHTML.SlotPlayData.Value));
        }
        #endregion

        #region メソッド
        /// <summary>
        /// 台データの取得
        /// </summary>
        /// <param name="storeID">取得したい店のID</param>
        /// <param name="dataDate">データを取得したい日付</param>
        /// <param name="slotMachineCount">その店の遊技台数</param>
        //public async Task GetSlotDataAsync(string storeID, DateTime dataDate, int slotMachineCount)
        //{
        //    var year = dataDate.Year;
        //    var month = dataDate.Month;
        //    var day = dataDate.Day;

        //    //_slotDataApiUrl = new Url($"http://slotdata.gear.host/api/slotdata?storeID={storeID}&dataDate={year}-{month}-{day}");
        //    try
        //    {
        //        for (var slotMachineNo = 1; slotMachineNo <= slotMachineCount; ++slotMachineNo)
        //        {
        //            _slotDataApiUrl = new Url($"https://daidata.goraggio.com/{storeID}/detail?unit={slotMachineNo}&target_date={year}-{month}-{day}");
        //            //URL内のソースを取得
        //            var response = await _httpClient.GetStreamAsync(_slotDataApiUrl);
        //            //取得したソースを解析
        //            await _analysisHTML.AnalyseAsync(response);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        if (e == new HttpRequestException())
        //        {
        //            //指定されたURLが不正です。
        //        }
        //        throw;
        //    }
        //}
        public async Task GetSlotDataAsync(DateTime dataDate)
        {
            var year = dataDate.Year;
            var month = dataDate.Month;
            var day = dataDate.Day;

            var tempos = new SlotAddictionDBContext().Tempos.ToList();


            //_slotDataApiUrl = new Url($"https://daidata.goraggio.com/{storeID}/detail?unit={slotMachineNo}&target_date={year}-{month}-{day}");
            try
            {
                foreach (var tempo in tempos)
                {
                    for (var slotMachineNo = tempo.SlotMachineStartNo; slotMachineNo <= tempo.SlotMachineEndNo; ++slotMachineNo)
                    {
                        _slotDataApiUrl = new Url($"{tempo.StoreURL}unit={slotMachineNo}&target_date={year}-{month}-{day}");
                        //URL内のソースを取得
                        var response = await _httpClient.GetStreamAsync(_slotDataApiUrl);
                        //取得したソースを解析
                        var analysisSlotData = await _analysisHTML.AnalyseAsync(response);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataDate"></param>
        /// <returns></returns>
        public async Task GetSlotDataAsync(DateTime dataDate, string slotModel)
        {
            var year = dataDate.Year;
            var month = dataDate.Month;
            var day = dataDate.Day;
            var tempos = new SlotAddictionDBContext().Tempos.ToList();

            //_slotDataApiUrl = new Url($"https://daidata.goraggio.com/{storeID}/detail?unit={slotMachineNo}&target_date={year}-{month}-{day}");
            try
            {
                foreach (var tempo in tempos)
                {
                    var startN0 = tempo.SlotMachineStartNo;
                    var endNo = tempo.SlotMachineEndNo;

                    if (slotModel != null)
                    {
                        //店舗のフロアURLを作成
                        var floorUrl = new Url($"{tempo.StoreURL}floor");

                        //URL内のソースを取得
                        var response = await _httpClient.GetStreamAsync(floorUrl);

                        //取得したソースを解析
                        var floorDataForSlotModel = await _analysisHTML.AnalyseFloorAsync(response, slotModel);
                        startN0 = floorDataForSlotModel.Min();
                        endNo = floorDataForSlotModel.Max();
                    }

                    for (var slotMachineNo = startN0; slotMachineNo <= endNo; ++slotMachineNo)
                    {
                        //リクエストを投げるURLを作成
                        _slotDataApiUrl = new Url($"{tempo.StoreURL}detail?unit={slotMachineNo}&target_date={year}-{month}-{day}");

                        //URL内のソースを取得
                        var response = await _httpClient.GetStreamAsync(_slotDataApiUrl);

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
