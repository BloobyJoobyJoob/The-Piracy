using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipUpgrades
{
    public ShipUpgradeTier[] shipTiers;
    public int tier {get; private set;} = 0;

    public void UpgradeShip(){
        tier += 1;

        if (tier <= shipTiers.Length)
        {
            ShipAttachment[] attachments = shipTiers[tier].shipAttachments;

            for (var i = 0; i < attachments.Length; i++)
            {
                if (attachments[i].gameObject.activeSelf)
                {
                    attachments[i].gameObject.transform.localScale = attachments[i].scale;
                }
                else
                {
                    attachments[i].gameObject.SetActive(true);
                }
            }

            foreach (GameObject gameObject in shipTiers[tier].removeShipAttachments)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Tried to upgrade past possible level");
        }
    }
}
[System.Serializable]
public class ShipUpgradeTier{
    public ShipAttachment[] shipAttachments;
    public GameObject[] removeShipAttachments;
}

[System.Serializable]
public class ShipAttachment{
    public GameObject gameObject;
    public Vector3 scale = Vector3.one;
}
