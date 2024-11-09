using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(100)]
public class WeaponDebugButton : MonoBehaviour
{
    [SerializeField] private TMP_Text weaponRareLabel;
    private Weapon weapon;

    void Start()
    {
        weapon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>().weapon;
    }

    void Update()
    {
        weaponRareLabel.text = weapon.weaponRare.ToString();
    }

    public void ChangeWeaponRare()
    {
        weapon.weaponRare = (
            (Weapon.WeaponRare)(((int)weapon.weaponRare + 1) % System.Enum.GetValues(typeof(Weapon.WeaponRare)).Length)
        );
    }


}
