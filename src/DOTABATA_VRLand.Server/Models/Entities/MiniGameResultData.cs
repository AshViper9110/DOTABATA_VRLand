namespace DOTABATA_VRLand.Server.Models.Entities {
    public class MiniGameResultData {
        // ミニゲームの順位リスト
        public List<int> rankings = new List<int>();
        // 勝利数
        public int winCount = 0;
        // 全体の順位
        public int allRoundRanking = -1;

        // ポイント
        public int point = 0;
        /*
         * 1位, 1p
         * 2位, 2p
         * 3位, 3p
         * 4位, 4p
         * 
         */
    }
}
