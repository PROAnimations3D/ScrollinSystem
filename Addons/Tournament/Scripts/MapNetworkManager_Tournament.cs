using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public sealed partial class MapNetworkManager
    {
        internal async void HandleTournamentFinish(MessageHandlerData messageHandler)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            TournamentFinishMessage message = messageHandler.ReadMessage<TournamentFinishMessage>();
            foreach (var item in message.finishs)
            {
                tournamentFinishs[item.dataId] = item;

                foreach (var itm in GameInstance.Singleton.Tournaments.Keys)
                {
                    if (itm == null)
                        continue;
                    if (itm.DataId == item.dataId)
                    {
                        itm.finished = item.finish;
                    }
                }
            }
            SendTournamentFinish();

            await UniTask.Yield();
#endif
        }

        internal async void HandleTournamentReset(MessageHandlerData messageHandler)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            TournamentResetMessage message = messageHandler.ReadMessage<TournamentResetMessage>();
            int data = message.tournament;
            foreach (var itm in GameInstance.Singleton.Tournaments.Keys)
            {
                if (itm == null)
                    continue;
                if (itm.DataId == data)
                {
                    itm.finished = false;
                    SendTournamentReset(data);
                }
            }
            await UniTask.Yield();
#endif
        }
    }
}
