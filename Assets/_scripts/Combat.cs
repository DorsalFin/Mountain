using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Combat : MonoBehaviour {

    private Character _character;

    public float maxLife = 100;
    private float _currentLife;
    public Image lifeImage;
    public float baseHealthRegenRate = 10;
    public float currentHealthRegenRate;
    private float _healthRegenTimer;

    public float CurrentLife { get { return _currentLife; } }

    public Item leftHandItem = null;
    public Item rightHandItem = null;

    public Image rightHandImage;
    public Image leftHandImage;

    public AudioSource leftHandSfx;
    public AudioSource rightHandSfx;

    // cached variables to make combat calculations more efficient
    private bool _fighting;
    private Character _target = null;
    private float _rightTimer;
    private float _leftTimer;
    private bool _swingingWithRightHand;
    private bool _swingingWithLeftHand;
    private float _rightAttackSpeed;
    private float _leftAttackSpeed;

    public float offHandWeaponSpeedMultiplier = 1.75f;

    void Awake()
    {
        _character = GetComponent<Character>();
        _currentLife = maxLife;
        currentHealthRegenRate = baseHealthRegenRate;
    }

    public void SetLife(float life) 
    {
        maxLife = life;
        _currentLife = life;
    }

    public void EngageTarget(Character target)
    {
        if (target.isDead)
            return;

        _target = target;
        _leftTimer = 0;
        _rightTimer = 0;
        _fighting = true;
    }

    public void Disengage(Character target)
    {
        _fighting = false;
        _target = null;

        target.UpdateAttackState();
    }

    public void EquipItem(Item item, bool rightHand)
    {
        if (rightHand)
        {
            rightHandItem = item;
            _rightTimer = 0;
            _rightAttackSpeed = item.GetValueFromAttributes("attack speed");
            _swingingWithRightHand = _rightAttackSpeed != 0;
            if (rightHandImage != null)
                rightHandImage.enabled = _swingingWithRightHand;
        }
        else
        {
            leftHandItem = item;
            _leftTimer = 0;
            // off hand weapons are slower
            _leftAttackSpeed = item.GetValueFromAttributes("attack speed") * offHandWeaponSpeedMultiplier;
            _swingingWithLeftHand = _leftAttackSpeed != 0;
            if (leftHandImage != null)
                leftHandImage.enabled = _swingingWithLeftHand;
        }
    }

    public void UnequipItem(Item item, bool rightHand)
    {
        if (rightHand)
        {
            rightHandItem = null;
            _swingingWithRightHand = false;
        }
        else
        {
            leftHandItem = null;
            _swingingWithLeftHand = false;
        }
    }

    void Update()
    {
        if (!_character.isDead && _currentLife < maxLife)
        {
            _healthRegenTimer += Time.deltaTime;
            if (_healthRegenTimer > currentHealthRegenRate)
            {
                _currentLife++;
                _healthRegenTimer = 0;
            }
        }

        if (lifeImage != null)
            lifeImage.fillAmount = CurrentLife / maxLife;

        if (leftHandImage && leftHandImage.enabled)
            leftHandImage.fillAmount = _leftTimer / _leftAttackSpeed;
        if (rightHandImage && rightHandImage.enabled)
            rightHandImage.fillAmount = _rightTimer / _rightAttackSpeed;

        if (_fighting && _target != null)
        {
            if (!_character.closestTile.inhabitants.Contains(_target) || _target.isDead)
            {
                Disengage(_target);
                return;
            }

            // right hand (main hand)
            if (_swingingWithRightHand)
            {
                _rightTimer += Time.deltaTime;
                
                if (_rightTimer > _rightAttackSpeed)
                {
                    bool struck = _target.combat.ProcessHit(rightHandItem, 0);
                    rightHandSfx.PlayOneShot(struck ? rightHandItem.hitAudio : rightHandItem.missAudio);
                    _rightTimer = 0;
                }
            }

            // left hand (off hand)
            if (_swingingWithLeftHand)
            {
                _leftTimer += Time.deltaTime;

                if (_leftTimer > _leftAttackSpeed)
                {
                    bool struck = _target.combat.ProcessHit(leftHandItem, 1);
                    leftHandSfx.PlayOneShot(struck ? leftHandItem.hitAudio : leftHandItem.missAudio);
                    _leftTimer = 0;
                }
            }
        }
    }

    public bool ProcessHit(Item itemHitBy, int hand)
    {
        float hitChance = itemHitBy.GetValueFromAttributes("hit chance");
        bool hitMissed = UnityEngine.Random.Range(0, 100) > hitChance;

        if (hitMissed)
        {
            return false;
        }

        // TODO check if the hit gets blocked

        float baseDamage = itemHitBy.GetValueFromAttributes("damage");

        // TODO modify damage by my armour value
        float modifiedDamage = baseDamage;

        // TODO check for any item on hit effects

        Debug.Log("HIT " +gameObject.name +" for " +modifiedDamage +" damage");

        _currentLife -= modifiedDamage;

        if (_currentLife <= 0)
        {
            if (_target != null)
                Disengage(_target);

            _character.isDead = true;
        }

        _character.PlayVoiceClip(_character.isDead ? "death" : "hurt");

        return true;
    }

    //public Item UpgradeWeapon(Item baseItem)
    //{
    //    Item upgradedItem = baseItem.getCopy();

    //    float newDamage = baseItem.GetValueFromAttributes("damage") * ((float)LevelParameters.Instance.weaponAttributeUpgradePercent / 100f);
    //    float newAccuracy = baseItem.GetValueFromAttributes("hit chance") * ((float)LevelParameters.Instance.weaponAttributeUpgradePercent / 100f);

    //    upgradedItem.SetValueInAttributes("damage", newDamage);
    //    upgradedItem.SetValueInAttributes("hit chance", newAccuracy);

    //    return upgradedItem;
    //}
}
