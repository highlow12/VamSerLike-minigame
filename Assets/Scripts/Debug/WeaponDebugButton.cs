using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(100)]
public class WeaponDebugButton : MonoBehaviour
{
    [SerializeField] private TMP_Text weaponRareLabel;
    private Weapon.MainWeapon weapon;
    private PlayerAttack playerAttack;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        weapon = player.GetComponent<PlayerAttack>().mainWeapon;
        playerAttack = player.GetComponent<PlayerAttack>();
    }

    void Update()
    {
        weaponRareLabel.text = weapon.weaponRare.ToString();
    }

    public void ApplyWeaponRare()
    {
        Weapon.WeaponRare weaponRare = playerAttack.GetWeaponRare(weapon.weaponType);
        weapon.weaponRare = weaponRare;
    }


}
