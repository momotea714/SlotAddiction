using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Reactive.Bindings;

namespace SlotAddiction.Models
{
    public class AnalysisHTML
    {
        #region プロパティ
        public ReactiveProperty<SlotPlayData> SlotPlayData { get; set; }
        public SlotPlayData SlotPlayData_ { get; set; }
        #endregion

        #region メソッド
        /// <summary>
        /// HTMLを解析します。
        /// </summary>
        /// <param name="html">解析したいHTMLを指定します。</param>
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html)
        {
            if (html == null) return null;

            SlotPlayData = new ReactiveProperty<SlotPlayData>();
            SlotPlayData_ = new SlotPlayData();

            // HTMLをAngleSharp.Parser.Html.HtmlParserオブジェクトパースさせる
            var parser = new HtmlParser();
            var doc = await parser.ParseAsync(html);

            //Bodyを取得
            var body = doc.QuerySelector("#Main-Contents");

            //台名を取得
            var id_pachinkoTi = body.QuerySelector("#pachinkoTi");
            var slotMachineTitle = id_pachinkoTi.QuerySelector("strong").TextContent;

            //以下のように取得される
            //(20円スロット                            | 1番台)
            var coinPriceAndMachineNO = id_pachinkoTi.QuerySelector("span").TextContent;
            var coinPriceAndMachineNOFormat = coinPriceAndMachineNO.Substring(1, coinPriceAndMachineNO.Length - 2).Split('|');
            for (var i = 0; i < coinPriceAndMachineNOFormat.Length; ++i)
            {
                coinPriceAndMachineNOFormat[i] = coinPriceAndMachineNOFormat.ElementAt(i).Trim();
            }

            //コイン単価を取得する
            var coinPricePosition = coinPriceAndMachineNOFormat.First().IndexOf('円');
            var coinPrice = coinPriceAndMachineNOFormat.First().Substring(0, coinPricePosition);

            //台番号を取得する
            var machineNOPosition = coinPriceAndMachineNOFormat.ElementAt(1).IndexOf('番');
            var machineNO = coinPriceAndMachineNOFormat.ElementAt(1).Substring(0, machineNOPosition);

            //最新更新日時を取得する
            var latestClass = body.QuerySelector(".latest");
            var latest = (latestClass == null) ? body.QuerySelector(".older").TextContent : latestClass.TextContent;

            //各回数を取得する
            var bbCount = body.QuerySelector(".Text-Big25").TextContent;
            var textBig19classes = body.QuerySelectorAll(".Text-Big19");
            var rbCount = textBig19classes.First().TextContent;
            var artCount = textBig19classes.ElementAt(1).TextContent;
            var startCount = textBig19classes.ElementAt(2).TextContent;

            return new SlotPlayData
            {
                Title = slotMachineTitle,
                CoinPrice = Convert.ToInt32(coinPrice),
                MachineNO = Convert.ToInt32(machineNO),
                LatestUpdateDatetime = latest,
                BigBonusCount = Convert.ToInt32(bbCount),
                RegulerBonusCount = Convert.ToInt32(rbCount),
                ARTCount = Convert.ToInt32(artCount),
                StartCount = Convert.ToInt32(startCount),
            };
        }
        /// <summary>
        /// HTMLを解析します。
        /// </summary>
        /// <param name="html">解析したいHTMLを指定します。</param>
        /// <param name="slotModel">解析したい機種を指定します。</param>
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html, string slotModel)
        {
            if (html == null) return null;

            // HTMLをAngleSharp.Parser.Html.HtmlParserオブジェクトパースさせる
            var parser = new HtmlParser();
            var doc = await parser.ParseAsync(html);

            //Bodyを取得
            var body = doc.QuerySelector("#Main-Contents");

            //台名を取得
            var id_pachinkoTi = body.QuerySelector("#pachinkoTi");
            var slotMachineTitle = id_pachinkoTi.QuerySelector("strong").TextContent;

            //指定した機種でなければ終了
            if (slotModel != null &&
                slotMachineTitle != slotModel)
            {
                return null;
            }

            //以下のように取得される
            //(20円スロット                            | 1番台)
            var coinPriceAndMachineNO = id_pachinkoTi.QuerySelector("span").TextContent;
            var coinPriceAndMachineNOFormat = coinPriceAndMachineNO.Substring(1, coinPriceAndMachineNO.Length - 2).Split('|');
            for (var i = 0; i < coinPriceAndMachineNOFormat.Length; ++i)
            {
                coinPriceAndMachineNOFormat[i] = coinPriceAndMachineNOFormat.ElementAt(i).Trim();
            }

            //コイン単価を取得する
            var coinPricePosition = coinPriceAndMachineNOFormat.First().IndexOf('円');
            var coinPrice = coinPriceAndMachineNOFormat.First().Substring(0, coinPricePosition);

            //台番号を取得する
            var machineNOPosition = coinPriceAndMachineNOFormat.ElementAt(1).IndexOf('番');
            var machineNO = coinPriceAndMachineNOFormat.ElementAt(1).Substring(0, machineNOPosition);

            //最新更新日時を取得する
            var latestClass = body.QuerySelector(".latest");
            var latest = (latestClass == null) ? body.QuerySelector(".older").TextContent : latestClass.TextContent;

            //各回数を取得する
            var bbCount = body.QuerySelector(".Text-Big25").TextContent;
            var textBig19classes = body.QuerySelectorAll(".Text-Big19");
            var rbCount = textBig19classes.First().TextContent;
            var artCount = textBig19classes.ElementAt(1).TextContent;
            var startCount = textBig19classes.ElementAt(2).TextContent;

            return new SlotPlayData
            {
                Title = slotMachineTitle,
                CoinPrice = Convert.ToInt32(coinPrice),
                MachineNO = Convert.ToInt32(machineNO),
                LatestUpdateDatetime = latest,
                BigBonusCount = Convert.ToInt32(bbCount),
                RegulerBonusCount = Convert.ToInt32(rbCount),
                ARTCount = Convert.ToInt32(artCount),
                StartCount = Convert.ToInt32(startCount),
            };
        }
        #endregion
    }
}