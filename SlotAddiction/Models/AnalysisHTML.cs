using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using SlotAddiction.Const;
using SlotAddiction.DataBase;

namespace SlotAddiction.Models
{
    public class AnalysisHTML
    {
        #region メソッド
        /// <summary>
        /// HTMLを解析します。
        /// </summary>
        /// <param name="html">解析したいHTMLを指定します。</param>
        /// <param name="tempo">店舗情報</param>
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html, Tempo tempo)
        {
            return await AnalyseAsync(html, tempo, null);
        }
        /// <summary>
        /// HTMLを解析します。
        /// </summary>
        /// <param name="html">解析したいHTMLを指定します。</param>
        /// <param name="tempo">店舗情報</param>
        /// <param name="slotModels">解析したい機種を指定します。</param>
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html, Tempo tempo, List<string> slotModels)
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
            if (slotModels != null &&
                !slotModels.Contains(slotMachineTitle))
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
            var class_TextBig25 = body.QuerySelector(".Text-Big25");
            if (class_TextBig25 == null) return null;
            var bbCount = class_TextBig25.TextContent;
            var textBig19classes = body.QuerySelectorAll(".Text-Big19");
            var rbCount = textBig19classes?.First().TextContent;
            var artCount = textBig19classes.ElementAt(1).TextContent;
            var startCount = textBig19classes.ElementAt(2).TextContent;

            //取得した機種がスルー回数を狙える機種ならばスルー回数を取得する
            var status = string.Empty;
            var winingHistory = new List<WiningType>();
            var db = new SlotAddictionDBContext();
            var slotModelInfo = db.SlotModels.SingleOrDefault(x => x.SlotModelName.Contains(slotMachineTitle));
            if (slotModelInfo != null
                && slotModelInfo.ThroughType == "Basilisk_Kizuna")
            {
                //大当たり履歴が格納されているnumericValueTableクラスを取得
                var class_numericValueTable = doc.QuerySelector(".numericValueTable");

                var trTagChildTdTags = class_numericValueTable.QuerySelectorAll("tr td");
                for (var i = 3; i < trTagChildTdTags.Length; i += 5)
                {
                    var winingType = (WiningType) Enum.Parse(typeof(WiningType), trTagChildTdTags[i].TextContent);
                    winingHistory.Add(winingType);
                }
            }

            if (winingHistory.Any())
            {
                if (winingHistory.First() == WiningType.ART
                    || winingHistory.First() == WiningType.RB)
                {
                    var throughCount = winingHistory.Skip(1).TakeWhile(x => x == WiningType.BB).Count();
                    if (throughCount == 10)
                    {
                        status = "ART + BB10回スルー";
                    }
                    else if (throughCount == 9)
                    {
                        status = "ART + BB9回スルー";
                    }
                }

                else
                {
                    var throughCount = winingHistory.TakeWhile(x => x == WiningType.BB).Count();
                    if (throughCount == 10)
                    {
                        status = "BB10回スルー";
                    }
                    else if (throughCount == 9)
                    {
                        status = "BB9回スルー";
                    }
                }
            }

            return new SlotPlayData
            {
                StoreName = tempo.StoreName,
                Title = slotMachineTitle,
                CoinPrice = Convert.ToDecimal(coinPrice),
                MachineNO = Convert.ToInt32(machineNO),
                LatestUpdateDatetime = latest,
                BigBonusCount = Convert.ToInt32(bbCount),
                RegulerBonusCount = Convert.ToInt32(rbCount),
                ARTCount = Convert.ToInt32(artCount),
                StartCount = Convert.ToInt32(startCount),
                Status = status,
                //WiningHistory = winingHistory,
            };
        }
        /// <summary>
        /// 指定した機種が何番から何番まで置いてあるのかを取得する
        /// </summary>
        /// <param name="html"></param>
        /// <param name="slotModels"></param>
        /// <returns></returns>
        public async Task<List<int>> AnalyseFloorAsync(Stream html, List<string> slotModels)
        {
            if (html == null || slotModels == null) return null;

            //指定した機種が存在する台番号を格納するリストを作成
            var slotModelUnits = new List<int>();

            // HTMLをAngleSharp.Parser.Html.HtmlParserオブジェクトパースさせる
            var parser = new HtmlParser();
            var doc = await parser.ParseAsync(html);

            //店内の機種一覧を取得
            var classes_sorterTablesorter = doc.QuerySelector(".sorter");

            var slotModelTrTd = classes_sorterTablesorter.QuerySelectorAll("tr td");

            for (var i = 1; i < slotModelTrTd.Length; i += 12)
            {
                var splitNewline = slotModelTrTd.ElementAt(i).TextContent.Split('\n');
                slotModelUnits.Add(Convert.ToInt32(Regex.Replace(splitNewline.ElementAt(1), @"\s", string.Empty)));
            }

            return slotModelUnits;
        }
        #endregion
    }
}