using _Scripts.Core.Regions;
using _Scripts.ResorcesAndRecipes;
using Mirror;

namespace _Scripts.Network.Client
{
    public class PlayerAvatar : NetworkBehaviour
    {
        public Storage Storage;

        #region Requests

        /// <summary>
        ///  запрашивает у сервера изменение текущего масштаба производства
        /// </summary>
        /// <param name="economyEntityID"></param>
        /// <param name="scale"></param>
        public void RequestSetProductionScale(int economyEntityID, int scale)
        {
            Client.Instance.CmdSetProductionScale(economyEntityID, scale);
        }

        /// <summary>
        ///  запрашивает у сервера добавление в регион нового производства по рецепту
        /// </summary>
        /// <param name="regionID"></param>
        /// <param name="recipeID"></param>
        public void RequestAddProduction(int regionID, int recipeID)
        {
            Client.Instance.CmdAddEconomyEntity(regionID, recipeID);
        }

        public void RequestUpgradeProduction(int economyEntityID)
        {
            Client.Instance.CmdUpgradeProduction(economyEntityID);
        }

        #endregion

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isLocalPlayer) Client.Instance.SetLocalPlayerAvatar(this);
            Storage = new Storage();
        }
    }
}