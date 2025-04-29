using UnityEngine;

[CreateAssetMenu(fileName = "SubWeaponSO", menuName = "Scriptable Objects/SubWeaponSO")]
public class SubWeaponSO : ScriptableObject
{
    [Header("SubWeapon")]
    public Weapon.SubWeapon.WeaponType weaponType;
    public int weaponGrade;
    [Header("AttackObject")]
    public Sprite weaponSprite;
    public AnimatorOverrideController weaponAnimator;
}
