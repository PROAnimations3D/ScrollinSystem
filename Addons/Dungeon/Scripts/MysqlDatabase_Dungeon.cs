#if UNITY_EDITOR || UNITY_SERVER
using LiteNetLibManager;
using MySqlConnector;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase : BaseDatabase
    {
        [DevExtMethods("ReadCharacter")]
        private void ReadCharacterDungeon(PlayerCharacterData result, bool test, bool test2, bool test3, bool test4, bool test5, bool test6, bool test7, bool test8, bool test9, bool test10, bool test11)
        {
            List<PlayerDungeon> dungeons = new List<PlayerDungeon>();
            ReadCharacterDungeons(result.Id, dungeons);
            result.Dungeons = dungeons;
        }

        private bool ReadCharacterDungeon(MySqlDataReader reader, out PlayerDungeon result)
        {
            if (reader.Read())
            {
                result = new PlayerDungeon();
                result.CharacterId = reader.GetString(0);
                result.DataId = reader.GetInt32(1);
                result.LoginTime = ((System.DateTimeOffset)reader.GetDateTime(2)).ToUnixTimeSeconds();
                return true;
            }
            result = PlayerDungeon.Empty;
            return false;
        }
        public List<PlayerDungeon> ReadCharacterDungeons(string characterId, List<PlayerDungeon> result = null)
        {
            if (result == null)
                result = new List<PlayerDungeon>();
            ExecuteReaderSync((reader) =>
            {
                PlayerDungeon tempDungeon;
                while (ReadCharacterDungeon(reader, out tempDungeon))
                {
                    result.Add(tempDungeon);
                }
            }, "SELECT characterId, dataId, loginTime FROM dungeon WHERE characterId=@characterId AND loginTime=UTC_DATE() ORDER BY idx ASC",
                new MySqlParameter("@characterId", characterId));
            return result;
        }

        [DevExtMethods("UpdateCharacter")]
        private void UpdateCharacterDungeon(IPlayerCharacterData characterData)
        {
            FillCharacterDungeons(characterData);
        }

        private void FillCharacterDungeons(IPlayerCharacterData characterData)
        {
            MySqlConnection connection = NewConnection();
            OpenConnectionSync(connection);
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                DeleteCharacterDungeon(connection, transaction, characterData.Id);
                int i;
                for (i = 0; i < characterData.Dungeons.Count; ++i)
                {
                    CreateCharacterDungeon(connection, transaction, i, characterData.Id, characterData.Dungeons[i]);
                }
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                Logging.LogError(ToString(), "Transaction, Error occurs while replacing dungeon of character: " + characterData.Id);
                Logging.LogException(ToString(), ex);
                transaction.Rollback();
            }
            transaction.Dispose();
            connection.Close();
        }
        public void CreateCharacterDungeon(MySqlConnection connection, MySqlTransaction transaction, int idx, string characterId, PlayerDungeon characterDungeon)
        {
            ExecuteNonQuerySync(connection, transaction, "INSERT INTO dungeon (id, idx, characterId, dataId, loginTime) VALUES (@id, @idx, @characterId, @dataId, @loginTime)",
                new MySqlParameter("@id", characterId + "_" + idx),
                new MySqlParameter("@idx", idx),
                new MySqlParameter("@characterId", characterId),
                new MySqlParameter("@dataId", characterDungeon.DataId),
                new MySqlParameter("@loginTime", System.DateTime.UtcNow.Date));
        }

        public void DeleteCharacterDungeon(MySqlConnection connection, MySqlTransaction transaction, string characterId)
        {
            ExecuteNonQuerySync(connection, transaction, "DELETE FROM dungeon WHERE characterId=@characterId", new MySqlParameter("@characterId", characterId));
        }
    }
}
#endif