using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
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
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html)
        {
            return await AnalyseAsync(html, null);
        }
        /// <summary>
        /// HTMLを解析します。
        /// </summary>
        /// <param name="html">解析したいHTMLを指定します。</param>
        /// <param name="slotModel">解析したい機種を指定します。</param>
        /// <returns></returns>
        public async Task<SlotPlayData> AnalyseAsync(Stream html, List<string> slotModel)
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
                !slotModel.Contains(slotMachineTitle))
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
            string throughType = null;
            var throughCount = 0;

            //取得した機種がスルー回数を狙える機種ならばスルー回数を取得する
            var db = new SlotAddictionDBContext();
            var slotmodels = db.SlotModels.Where(x => x.SlotModelName.Contains(slotMachineTitle));
            if (slotmodels.Any())
            {
                throughType = slotmodels.Single().ThroughType;
                //throughType = "BB";
            }

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
        /// 指定した機種が何番から何番まで置いてあるのかを取得する
        /// </summary>
        /// <param name="html"></param>
        /// <param name="slotModel"></param>
        /// <returns></returns>
        public async Task<List<int>> AnalyseFloorAsync(Stream html, List<string> slotModel)
        {
            if (html == null || slotModel == null) return null;

            //指定した機種が存在する台番号を格納するリストを作成
            var slotModelUnits = new List<int>();

            // HTMLをAngleSharp.Parser.Html.HtmlParserオブジェクトパースさせる
            var parser = new HtmlParser();
            var doc = await parser.ParseAsync(html);

            //店内の機種一覧を取得
            var animatedModalShowUnitName = doc.QuerySelector("#animatedModalShowUnitName .list2col");

            //以下のようなデータが設置機種分取得される
            //<ul>
            //<li class="Slot">
            //<a href = "https://daidata.goraggio.com/100359/floor?rank=1F&bid=1&model=%EF%BE%8F%EF%BD%B2%EF%BD%BC%EF%BE%9E%EF%BD%AC%EF%BD%B8%EF%BE%9E%EF%BE%97%EF%BD%B0II" >< h2 >
            //<h2>ﾏｲｼﾞｬｸﾞﾗｰII</h2>
            //85～96                                        </a>
            //</li>
            //</ul>
            var slotModelList = animatedModalShowUnitName.QuerySelectorAll("ul");

            //全てのulタグ内の機種名から指定した機種名に合致するデータを取得する
            var datas = slotModelList.Where(x => slotModel.Contains(x.QuerySelectorAll("h2").ElementAt(1).TextContent)).ToList();

            foreach(var data in datas)
            {
                var splitNewline = data.QuerySelector("a").Text().Split('\n');

                //指定した機種名が存在する台番号を取得する
                //「85～96」のように取得される
                var units = Regex.Replace(splitNewline.ElementAt(2), @"\s", string.Empty);

                //同機種が散り散りに配置されている場合はカンマ区切りで取得される為に台番号の分解を行う
                var separationUnits = units.Split(',');

                foreach (var unit in separationUnits)
                {
                    //台番号の中に「～」が入っていれば隣り合って同機種が設置されていると判定する
                    if (unit.Contains('～'))
                    {
                        var split = unit.Split('～').Select(int.Parse);
                        var rangeStartNo = split.First();
                        slotModelUnits.AddRange(Enumerable.Range(rangeStartNo, split.ElementAt(1) - rangeStartNo + 1));
                    }
                    else
                    {
                        slotModelUnits.Add(Convert.ToInt32(unit));
                    }
                }
            }

            return slotModelUnits;
        }
        #endregion
    }
}