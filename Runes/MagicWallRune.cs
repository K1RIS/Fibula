using UnityEngine;

public class MagicWallRune : Rune
{        
    override public void use(Player player)
    { 
        Vector3 position = Utility.convertMousePositionToVector3("Terrain");
        if (position != Vector3.zero)
        {
            if (!Physics.CheckSphere(position, 0.1f))
            {
                 player.photonView.RPC("createMagicWall", PhotonTargets.All, position);
            }
        }
    }
}
