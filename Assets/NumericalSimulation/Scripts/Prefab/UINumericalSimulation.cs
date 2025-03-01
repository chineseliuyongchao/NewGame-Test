using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.Script;
using Random = System.Random;

namespace NumericalSimulation.Scripts.Prefab
{
    public class UINumericalSimulation : MonoBehaviour
    {
        public TextAsset data;
        public UIChooseArmy team1Choose;
        private int _team1ArmyId;
        public UIChooseArmy team2Choose;
        private int _team2ArmyId;

        /// <summary>
        /// 选择进攻形式（0：单向攻击，1：互相攻击，2：互相进攻）
        /// </summary>
        public Dropdown attackFormDropdown;

        private AttackFormType _attackFormType;

        public Toggle moraleToggle;

        /// <summary>
        /// 在模拟时是否计算作战意志
        /// </summary>
        private bool _hasMorale;

        public Toggle fatigueToggle;

        /// <summary>
        /// 在模拟时是否计算作战意志
        /// </summary>
        private bool _hasFatigue;

        public Toggle chargeToggle;

        /// <summary>
        /// 进攻方是否冲锋，只有选择互相攻击时才有用
        /// </summary>
        private bool _hasCharge;

        public Toggle stickToggle;

        /// <summary>
        /// 防御方是否坚守，只有选择互相攻击，单向射击时才有用
        /// </summary>
        private bool _hasStick;

        /// <summary>
        /// 开始模拟
        /// </summary>
        public Button startImitateButton;

        /// <summary>
        /// 所有兵种数据
        /// </summary>
        private Dictionary<int, ArmDataType> _armDataTypes;

        public static Dictionary<string, string> ArmyAttribute;

        private void Awake()
        {
            InitArmyAttribute();
            _armDataTypes = new Dictionary<int, ArmDataType>();
            GameUtility.AnalysisJsonConfigurationTable(data, _armDataTypes);
            team1Choose.OnInit(_armDataTypes, i => _team1ArmyId = i);
            team2Choose.OnInit(_armDataTypes, i => _team2ArmyId = i);

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>
            {
                new("单向攻击"),
                new("互相攻击"),
                new("互相进攻"),
                new("单向射击"),
                new("互相射击")
            };
            // 将选项列表添加到Dropdown组件
            attackFormDropdown.options = options;
            // 设置默认选项（可选）
            attackFormDropdown.value = 0;
            // 添加监听器，处理选项改变事件
            attackFormDropdown.onValueChanged.AddListener(type => _attackFormType = (AttackFormType)type);

            moraleToggle.onValueChanged.AddListener(value => _hasMorale = value);
            fatigueToggle.onValueChanged.AddListener(value => _hasFatigue = value);
            chargeToggle.onValueChanged.AddListener(value => _hasCharge = value);
            stickToggle.onValueChanged.AddListener(value => _hasStick = value);

            startImitateButton.onClick.AddListener(StartImitate);
        }

        /// <summary>
        /// 开始攻击模拟
        /// </summary>
        private void StartImitate()
        {
            int index = 0;
            ArmData arm1 = new ArmData(_armDataTypes[_team1ArmyId], _team1ArmyId);
            ArmData arm2 = new ArmData(_armDataTypes[_team2ArmyId], _team2ArmyId);
            while (true) //暂时默认一次循环为一个回合
            {
                index++;
                ArmData arm1Old = new ArmData(arm1);
                ArmData arm2Old = new ArmData(arm2);
                switch (_attackFormType)
                {
                    case AttackFormType.ONE_WAY_ATTACK:
                    {
                        //单位1进攻单位2，单位2未反击
                        OneAttack(arm1, arm2);
                    }
                        break;
                    case AttackFormType.MUTUAL_ATTACK:
                    {
                        if (_hasCharge)
                        {
                            arm1.isCharge = true;
                        }

                        if (_hasStick)
                        {
                            arm2.isStick = true;
                        }

                        //单位1进攻单位2，单位2反击
                        ArmData arm2Before = new ArmData(arm2);
                        OneAttack(arm1, arm2);
                        OneAttack(arm2Before, arm1);
                    }
                        break;
                    case AttackFormType.MUTUAL_OFFENSE:
                    {
                        //单位1进攻单位2，单位2反击
                        ArmData arm2Before = new ArmData(arm2);
                        OneAttack(arm1, arm2);
                        OneAttack(arm2Before, arm1);
                        //单位2进攻单位1，单位1反击
                        ArmData arm1Before = new ArmData(arm1);
                        OneAttack(arm2, arm1);
                        OneAttack(arm1Before, arm2);
                    }
                        break;
                    case AttackFormType.ONE_WAY_SHOOTING:
                    {
                        if (_hasStick)
                        {
                            arm2.isStick = true;
                        }

                        //单位1射击单位2
                        OneShoot(arm1, arm2);
                    }
                        break;
                    case AttackFormType.MUTUAL_SHOOTING:
                    {
                        //单位1射击单位2，单位2再射击单位1
                        OneShoot(arm1, arm2);
                        OneShoot(arm2, arm1);
                    }
                        break;
                }

                PrintAttackResult(arm1Old, arm1, arm2Old, arm2, index);
                if (arm1.NowTroops <= 0 || arm2.NowTroops <= 0 || index > 50)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 输出攻击结果
        /// </summary>
        /// <param name="arm1Old"></param>
        /// <param name="arm1"></param>
        /// <param name="arm2Old"></param>
        /// <param name="arm2"></param>
        /// <param name="index"></param>
        private void PrintAttackResult(ArmData arm1Old, ArmData arm1, ArmData arm2Old, ArmData arm2, int index)
        {
            string result = "在第" + index + "回合：  \t";
            switch (_attackFormType)
            {
                case AttackFormType.ONE_WAY_ATTACK:
                {
                    result += "单位1对单位2发动了进攻，但是单位2没有反击。  \t";
                }
                    break;
                case AttackFormType.MUTUAL_ATTACK:
                {
                    result += "单位1对单位2发动了进攻，单位2进行了反击。  \t";
                }
                    break;
                case AttackFormType.MUTUAL_OFFENSE:
                {
                    result += "单位1对单位2发动了进攻，单位2进行了反击。随后单位2也对单位1发动了进攻，单位1也进行了反击。  \t";
                }
                    break;
                case AttackFormType.ONE_WAY_SHOOTING:
                {
                    result += "单位1对单位2发动了射击。  \t";
                }
                    break;
                case AttackFormType.MUTUAL_SHOOTING:
                {
                    result += "单位1对单位2发动了射击，单位2也对单位1发动了射击。  \t";
                }
                    break;
            }

            result += "单位1损失了" + (arm1Old.NowHp - arm1.NowHp) + "血量。损失了" + (arm1Old.NowTroops - arm1.NowTroops) +
                      "名士兵。单位1还剩余" + arm1.NowHp + "血量以及" + arm1.NowTroops + "名士兵。  \t";
            result += "单位2损失了" + (arm2Old.NowHp - arm2.NowHp) + "血量。损失了" + (arm2Old.NowTroops - arm2.NowTroops) +
                      "名士兵。单位2还剩余" + arm2.NowHp + "血量以及" + arm2.NowTroops + "名士兵。  \t";
            Debug.Log(result);
        }

        /// <summary>
        /// 一次攻击，默认a是攻击方，b是被攻击方
        /// </summary>
        /// <param name="armA">单位a</param>
        /// <param name="armB">单位b</param>
        private void OneAttack(ArmData armA, ArmData armB)
        {
            //计算命中次数
            int realAttack = RealAttack(armA);
            int realDefenseMelee = RealDefenseMelee(armB);
            float hitProbability = Math.Max(0.05f, Math.Min(1, realAttack / (realDefenseMelee * 3f))); //命中概率
            int successAttackNum = CompleteSuccessAttackNum(hitProbability, armA.NowTroops); //成功命中次数
            Debug.Log("命中概率：" + hitProbability + "  成功命中次数：" + successAttackNum);

            //计算单次实际杀伤（普通杀伤和破甲杀伤）
            float realMeleeNormal =
                Math.Max(_armDataTypes[armA.armId].meleeNormal - _armDataTypes[armB.armId].armor / 2, 0); //实际普通杀伤
            int armRealMeleeArmor = this.RealMeleeArmor(armA); //兵种的破甲杀伤修正
            float realMeleeArmorFactor = Math.Max(0.1f, Math.Min(1, //实际破甲杀伤系数
                1 - (_armDataTypes[armB.armId].armor - (float)armRealMeleeArmor) / armRealMeleeArmor * 0.15f));
            float realMeleeArmor = armRealMeleeArmor * realMeleeArmorFactor; //实际破甲杀伤
            Debug.Log("实际普通杀伤：" + realMeleeNormal + "  实际破甲杀伤：" + realMeleeArmor + "  实际破甲杀伤系数：" +
                      realMeleeArmorFactor);

            //计算实际攻击伤害
            int totalDamage = (int)(successAttackNum * (realMeleeNormal + realMeleeArmor)); //攻击产生的总伤害
            armB.NowHp -= totalDamage; //计算剩余血量
            int theoryMaxNum = _armDataTypes[armB.armId].totalTroops; //理论最大人数
            int theoryMinNum =
                (int)Math.Ceiling(theoryMaxNum * ((float)armB.NowHp / _armDataTypes[armB.armId].totalHp)); //理论最小人数
            float computeTroopsFactor = 0.7f; //剩余人数计算系数
            int theoryNowTroops = theoryMinNum + (int)((theoryMaxNum - theoryMinNum) * Math.Pow(armB.NowHp /
                (float)_armDataTypes[armB.armId].totalHp, computeTroopsFactor)); //剩余理论人数
            armB.NowTroops = Math.Max(theoryMinNum, Math.Min(theoryMaxNum, theoryNowTroops)); //剩余实际人数
            Debug.Log("攻击产生的总伤害：" + totalDamage + "  理论最大人数：" + theoryMaxNum + "  理论最小人数：" + theoryMinNum +
                      "  剩余理论人数：" + theoryNowTroops);
        }

        /// <summary>
        /// 一次射击，默认a是攻击方，b是被攻击方
        /// </summary>
        /// <param name="armA">单位a</param>
        /// <param name="armB">单位b</param>
        private void OneShoot(ArmData armA, ArmData armB)
        {
            //计算命中次数
            int realAccuracy = RealAccuracy(armA);
            int realDefenseRange = RealDefenseRange(armB);
            float hitProbability = Math.Max(0.05f, Math.Min(1, realAccuracy / (realDefenseRange * 3f))); //命中概率
            int successAttackNum = CompleteSuccessAttackNum(hitProbability, armA.NowTroops); //成功命中次数
            Debug.Log("命中概率：" + hitProbability + "  成功命中次数：" + successAttackNum);

            //计算单次实际杀伤（普通杀伤和破甲杀伤）
            float realRangeNormal =
                Math.Max(_armDataTypes[armA.armId].rangeNormal - _armDataTypes[armB.armId].armor / 2, 0); //实际普通杀伤
            int armRealRangeArmor = this.RealRangeArmor(armA); //兵种的破甲杀伤修正
            float realRangeArmorFactor = Math.Max(0.1f, Math.Min(1, //实际破甲杀伤系数
                1 - (_armDataTypes[armB.armId].armor - (float)armRealRangeArmor) / armRealRangeArmor * 0.15f));
            float realRangeArmor = armRealRangeArmor * realRangeArmorFactor; //实际破甲杀伤
            Debug.Log("实际普通杀伤：" + realRangeNormal + "  实际破甲杀伤：" + realRangeArmor + "  实际破甲杀伤系数：" +
                      realRangeArmorFactor);

            //计算实际攻击伤害
            int totalDamage = (int)(successAttackNum * (realRangeNormal + realRangeArmor)); //攻击产生的总伤害
            armB.NowHp -= totalDamage; //计算剩余血量
            int theoryMaxNum = _armDataTypes[armB.armId].totalTroops; //理论最大人数
            int theoryMinNum =
                (int)Math.Ceiling(theoryMaxNum * ((float)armB.NowHp / _armDataTypes[armB.armId].totalHp)); //理论最小人数
            float computeTroopsFactor = 0.7f; //剩余人数计算系数
            int theoryNowTroops = theoryMinNum + (int)((theoryMaxNum - theoryMinNum) * Math.Pow(armB.NowHp /
                (float)_armDataTypes[armB.armId].totalHp, computeTroopsFactor)); //剩余理论人数
            armB.NowTroops = Math.Max(theoryMinNum, Math.Min(theoryMaxNum, theoryNowTroops)); //剩余实际人数
            Debug.Log("攻击产生的总伤害：" + totalDamage + "  理论最大人数：" + theoryMaxNum + "  理论最小人数：" + theoryMinNum +
                      "  剩余理论人数：" + theoryNowTroops);
        }

        /// <summary>
        /// 计算实际攻击能力，计算规则原则上先加后乘
        /// </summary>
        /// <param name="armData"></param>
        /// <returns></returns>
        private int RealAttack(ArmData armData)
        {
            int correctAttack = _armDataTypes[armData.armId].attack;
            if (armData.isCharge)
            {
                correctAttack += _armDataTypes[armData.armId].charge;
            }

            if (armData.isStick)
            {
                correctAttack = (int)(correctAttack * 0.75f);
            }

            int realAttack = correctAttack; //修正后攻击能力
            if (_hasMorale)
            {
                realAttack = ComputeCorrectMorale(realAttack, armData);
            }

            if (_hasFatigue)
            {
                realAttack = ComputeCorrectFatigue(realAttack, armData);
            }

            return realAttack;
        }

        /// <summary>
        /// 计算实际近战防御能力，计算规则原则上先加后乘
        /// </summary>
        /// <param name="armData"></param>
        /// <returns></returns>
        private int RealDefenseMelee(ArmData armData)
        {
            int correctDefenseMelee = _armDataTypes[armData.armId].defenseMelee;
            if (armData.isStick)
            {
                correctDefenseMelee = (int)(correctDefenseMelee * 1.4f);
            }

            int realDefenseMelee = correctDefenseMelee; //修正后防御能力
            if (_hasMorale)
            {
                realDefenseMelee = ComputeCorrectMorale(realDefenseMelee, armData);
            }

            if (_hasFatigue)
            {
                realDefenseMelee = ComputeCorrectFatigue(realDefenseMelee, armData);
            }

            return realDefenseMelee;
        }

        /// <summary>
        /// 计算实际破甲杀伤，计算规则原则上先加后乘
        /// </summary>
        /// <param name="armData"></param>
        /// <returns></returns>
        private int RealMeleeArmor(ArmData armData)
        {
            int correctMeleeArmor = _armDataTypes[armData.armId].meleeArmor;
            if (armData.isCharge)
            {
                correctMeleeArmor += (int)(_armDataTypes[armData.armId].charge * 0.1f);
            }

            int realMeleeArmor = correctMeleeArmor;
            return realMeleeArmor;
        }

        /// <summary>
        /// 计算实际射击精度，计算规则原则上先加后乘
        /// </summary>
        /// <param name="armData"></param>
        /// <returns></returns>
        private int RealAccuracy(ArmData armData)
        {
            int correctAccuracy = _armDataTypes[armData.armId].accuracy;
            int realAccuracy = correctAccuracy;
            if (_hasMorale)
            {
                realAccuracy = ComputeCorrectMorale(realAccuracy, armData);
            }

            if (_hasFatigue)
            {
                realAccuracy = ComputeCorrectFatigue(realAccuracy, armData);
            }

            return realAccuracy;
        }

        /// <summary>
        /// 计算实际远程防御能力
        /// </summary>
        /// <param name="armData"></param>
        /// <returns></returns>
        private int RealDefenseRange(ArmData armData)
        {
            int correctDefenseRange = _armDataTypes[armData.armId].defenseRange;
            int realDefenseRange = correctDefenseRange;
            if (_hasMorale)
            {
                realDefenseRange = ComputeCorrectMorale(realDefenseRange, armData);
            }

            if (_hasFatigue)
            {
                realDefenseRange = ComputeCorrectFatigue(realDefenseRange, armData);
            }

            return realDefenseRange;
        }

        /// <summary>
        /// 计算作战意志修正
        /// </summary>
        private int ComputeCorrectMorale(int value, ArmData armData)
        {
            return (int)(value * (1 - (_armDataTypes[armData.armId].maximumMorale - armData.NowMorale) /
                (float)_armDataTypes[armData.armId].maximumMorale * 0.2f));
        }

        /// <summary>
        /// 计算疲劳值修正
        /// </summary>
        private int ComputeCorrectFatigue(int value, ArmData armData)
        {
            return (int)(value * (1 - armData.NowFatigue /
                (float)_armDataTypes[armData.armId].maximumFatigue * 0.5f));
        }

        /// <summary>
        /// 计算实际远程破甲杀伤
        /// </summary>
        /// <param name="armData"></param>
        /// <returns></returns>
        private int RealRangeArmor(ArmData armData)
        {
            int correctRangeArmor = _armDataTypes[armData.armId].rangeArmor;
            int realRangeArmor = correctRangeArmor;
            return realRangeArmor;
        }

        /// <summary>
        /// 计算命中次数
        /// </summary>
        /// <param name="hitProbability">命中概率</param>
        /// <param name="nowTroops">人数</param>
        /// <returns></returns>
        private int CompleteSuccessAttackNum(float hitProbability, int nowTroops)
        {
            int successAttackNum = 0;
            Random random = new Random();
            for (int i = 0; i < nowTroops; i++)
            {
                // 生成0到1之间的随机数
                float randomValue = (float)random.NextDouble();

                // 如果随机数小于命中概率，则计为命中
                if (randomValue < hitProbability)
                {
                    successAttackNum++;
                }
            }

            return successAttackNum;
        }

        /// <summary>
        /// 可以通过属性的英文名称找到中文，因为是工具，就直接写死了
        /// </summary>
        private static void InitArmyAttribute()
        {
            ArmyAttribute = new Dictionary<string, string>
            {
                { nameof(ArmDataType.unitName), "兵种名称" },
                { nameof(ArmDataType.totalHp), "总血量" },
                { nameof(ArmDataType.totalTroops), "总人数" },
                { nameof(ArmDataType.attack), "攻击能力" },
                { nameof(ArmDataType.charge), "冲锋加成" },
                { nameof(ArmDataType.defenseMelee), "防御能力（近战）" },
                { nameof(ArmDataType.defenseRange), "防御能力（远程）" },
                { nameof(ArmDataType.meleeNormal), "近战杀伤（普通）" },
                { nameof(ArmDataType.meleeArmor), "近战杀伤（破甲）" },
                { nameof(ArmDataType.armor), "护甲强度" },
                { nameof(ArmDataType.mobility), "移动能力" },
                { nameof(ArmDataType.sight), "视野" },
                { nameof(ArmDataType.stealth), "隐蔽" },
                { nameof(ArmDataType.ammo), "弹药量" },
                { nameof(ArmDataType.range), "射程" },
                { nameof(ArmDataType.reload), "装填速度" },
                { nameof(ArmDataType.accuracy), "精度" },
                { nameof(ArmDataType.rangeNormal), "远程杀伤（普通）" },
                { nameof(ArmDataType.rangeArmor), "远程杀伤（破甲）" },
                { nameof(ArmDataType.maximumMorale), "作战意志" },
                { nameof(ArmDataType.maximumFatigue), "疲劳值" },
                { nameof(ArmDataType.cost), "价格" }
            };
        }
    }
}