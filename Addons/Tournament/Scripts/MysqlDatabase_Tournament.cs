#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase : BaseDatabase
    {
        [DevExtMethods("ReadCharacter")]
        private void ReadCharacterTournament(PlayerCharacterData result, bool t, bool t2, bool t3, bool t4, bool t5, bool t6, bool t7, bool t8, bool t9, bool t10, bool t11)
        {
            result.TournamentGM = ReadTournamentGM(result.UserId);
        }

        public int ReadTournamentGM(string id)
        {
            int result = 0;
            ExecuteReaderSync((reader) =>
            {
                if (reader.Read())
                {
                    result = reader.GetInt32(0);
                }
            }, "SELECT userLevel FROM userlogin WHERE id=@id LIMIT 1",
                new MySqlParameter("@id", id));
            return result;
        }
    }
}
#endif