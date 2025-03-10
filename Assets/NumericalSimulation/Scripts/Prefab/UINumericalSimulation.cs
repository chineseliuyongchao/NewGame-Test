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

        //标准作战意志
        public const int INIT_MORALE = 50;

        //标准疲劳值
        public const int INIT_FATIGUE = 50;

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
            UnitData unitA = new UnitData(_armDataTypes[_team1ArmyId], _team1ArmyId);
            UnitData unitB = new UnitData(_armDataTypes[_team2ArmyId], _team2ArmyId);
            while (true) //暂时默认一次循环为一个回合
            {
                index++;
                UnitData unitAOld = new UnitData(unitA);
                UnitData unitBOld = new UnitData(unitB);
                switch (_attackFormType)
                {
                    case AttackFormType.ONE_WAY_ATTACK:
                    {
                        //单位1进攻单位2，单位2未反击
                        OneAttack(unitA, unitB);
                        AttackChangeMorale(unitBOld.NowTroops - unitB.NowTroops, unitB);
                    }
                        break;
                    case AttackFormType.MUTUAL_ATTACK:
                    {
                        if (_hasCharge)
                        {
                            unitA.isCharge = true;
                        }

                        if (_hasStick)
                        {
                            unitB.isStick = true;
                        }

                        //单位1进攻单位2，单位2反击
                        UnitData unitBBefore = new UnitData(unitB);
                        OneAttack(unitA, unitB);
                        OneAttack(unitBBefore, unitA);
                        AttackChangeMorale(unitAOld.NowTroops - unitA.NowTroops, unitA);
                        AttackChangeMorale(unitBOld.NowTroops - unitB.NowTroops, unitB);
                    }
                        break;
                    case AttackFormType.MUTUAL_OFFENSE:
                    {
                        //单位1进攻单位2，单位2反击
                        UnitData arm2Before = new UnitData(unitB);
                        OneAttack(unitA, unitB);
                        OneAttack(arm2Before, unitA);
                        AttackChangeMorale(unitAOld.NowTroops - unitA.NowTroops, unitA);
                        AttackChangeMorale(unitBOld.NowTroops - unitB.NowTroops, unitB);
                        UnitData unitAStaging = new UnitData(unitA);
                        UnitData unitBStaging = new UnitData(unitB);
                        //单位2进攻单位1，单位1反击
                        UnitData arm1Before = new UnitData(unitA);
                        OneAttack(unitB, unitA);
                        OneAttack(arm1Before, unitB);
                        AttackChangeMorale(unitAStaging.NowTroops - unitA.NowTroops, unitA);
                        AttackChangeMorale(unitBStaging.NowTroops - unitB.NowTroops, unitB);
                    }
                        break;
                    case AttackFormType.ONE_WAY_SHOOTING:
                    {
                        if (_hasStick)
                        {
                            unitB.isStick = true;
                        }

                        //单位1射击单位2
                        OneShoot(unitA, unitB);
                        AttackChangeMorale(unitBOld.NowTroops - unitB.NowTroops, unitB);
                    }
                        break;
                    case AttackFormType.MUTUAL_SHOOTING:
                    {
                        //单位1射击单位2，单位2再射击单位1
                        OneShoot(unitA, unitB);
                        OneShoot(unitB, unitA);
                        AttackChangeMorale(unitAOld.NowTroops - unitA.NowTroops, unitA);
                        AttackChangeMorale(unitBOld.NowTroops - unitB.NowTroops, unitB);
                    }
                        break;
                }

                PrintAttackResult(unitAOld, unitA, unitBOld, unitB, index);
                if (unitA.NowTroops <= 0 || unitB.NowTroops <= 0 || index > 50)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 输出攻击结果
        /// </summary>
        /// <param name="unit1Old"></param>
        /// <param name="unit1"></param>
        /// <param name="unit2Old"></param>
        /// <param name="unit2"></param>
        /// <param name="index"></param>
        private void PrintAttackResult(UnitData unit1Old, UnitData unit1, UnitData unit2Old, UnitData unit2, int index)
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

            result += "单位1损失了" + (unit1Old.NowHp - unit1.NowHp) + "血量。损失了" + (unit1Old.NowTroops - unit1.NowTroops) +
                      "名士兵。单位1还剩余" + unit1.NowHp + "血量以及" + unit1.NowTroops + "名士兵。当前的作战意志是" + unit1.NowMorale + "  \t";
            result += "单位2损失了" + (unit2Old.NowHp - unit2.NowHp) + "血量。损失了" + (unit2Old.NowTroops - unit2.NowTroops) +
                      "名士兵。单位2还剩余" + unit2.NowHp + "血量以及" + unit2.NowTroops + "名士兵。当前的作战意志是" + unit2.NowMorale + "  \t";
            Debug.Log(result);
        }

        /// <summary>
        /// 一次攻击，默认a是攻击方，b是被攻击方
        /// </summary>
        /// <param name="unitA">单位a</param>
        /// <param name="unitB">单位b</param>
        private void OneAttack(UnitData unitA, UnitData unitB)
        {
            //计算命中次数
            int realAttack = RealAttack(unitA);
            int realDefenseMelee = RealDefenseMelee(unitB);
            float hitProbability = Math.Max(0.05f, Math.Min(1, realAttack / (realDefenseMelee * 3f))); //命中概率
            int successAttackNum = CompleteSuccessAttackNum(hitProbability, unitA.NowTroops); //成功命中次数
            Debug.Log("实际攻击能力：" + realAttack + "  实际防御能力：" + realDefenseMelee + "  命中概率：" + hitProbability +
                      "  成功命中次数：" + successAttackNum);

            //计算单次实际杀伤（普通杀伤和破甲杀伤）
            float realMeleeNormal =
                Math.Max(_armDataTypes[unitA.armId].meleeNormal - _armDataTypes[unitB.armId].armor / 2, 0); //实际普通杀伤
            int armRealMeleeArmor = this.RealMeleeArmor(unitA); //兵种的破甲杀伤修正
            float realMeleeArmorFactor = Math.Max(0.1f, Math.Min(1, //实际破甲杀伤系数
                1 - (_armDataTypes[unitB.armId].armor - (float)armRealMeleeArmor) / armRealMeleeArmor * 0.15f));
            float realMeleeArmor = armRealMeleeArmor * realMeleeArmorFactor; //实际破甲杀伤
            Debug.Log("实际普通杀伤：" + realMeleeNormal + "  实际破甲杀伤：" + realMeleeArmor + "  实际破甲杀伤系数：" +
                      realMeleeArmorFactor);

            //计算实际攻击伤害
            int totalDamage = (int)(successAttackNum * (realMeleeNormal + realMeleeArmor)); //攻击产生的总伤害
            unitB.NowHp -= totalDamage; //计算剩余血量
            int theoryMaxNum = _armDataTypes[unitB.armId].totalTroops; //理论最大人数
            int theoryMinNum =
                (int)Math.Ceiling(theoryMaxNum * ((float)unitB.NowHp / _armDataTypes[unitB.armId].totalHp)); //理论最小人数
            float computeTroopsFactor = 0.7f; //剩余人数计算系数
            int theoryNowTroops = theoryMinNum + (int)((theoryMaxNum - theoryMinNum) * Math.Pow(unitB.NowHp /
                (float)_armDataTypes[unitB.armId].totalHp, computeTroopsFactor)); //剩余理论人数
            unitB.NowTroops = Math.Max(theoryMinNum, Math.Min(theoryMaxNum, theoryNowTroops)); //剩余实际人数
            Debug.Log("攻击产生的总伤害：" + totalDamage + "  理论最大人数：" + theoryMaxNum + "  理论最小人数：" + theoryMinNum +
                      "  剩余理论人数：" + theoryNowTroops);
        }

        /// <summary>
        /// 一次射击，默认a是攻击方，b是被攻击方
        /// </summary>
        /// <param name="unitA">单位a</param>
        /// <param name="unitB">单位b</param>
        private void OneShoot(UnitData unitA, UnitData unitB)
        {
            //计算命中次数
            int realAccuracy = RealAccuracy(unitA);
            int realDefenseRange = RealDefenseRange(unitB);
            float hitProbability = Math.Max(0.05f, Math.Min(1, realAccuracy / (realDefenseRange * 3f))); //命中概率
            int successAttackNum = CompleteSuccessAttackNum(hitProbability, unitA.NowTroops); //成功命中次数
            Debug.Log("命中概率：" + hitProbability + "  成功命中次数：" + successAttackNum);

            //计算单次实际杀伤（普通杀伤和破甲杀伤）
            float realRangeNormal =
                Math.Max(_armDataTypes[unitA.armId].rangeNormal - _armDataTypes[unitB.armId].armor / 2, 0); //实际普通杀伤
            int armRealRangeArmor = this.RealRangeArmor(unitA); //兵种的破甲杀伤修正
            float realRangeArmorFactor = Math.Max(0.1f, Math.Min(1, //实际破甲杀伤系数
                1 - (_armDataTypes[unitB.armId].armor - (float)armRealRangeArmor) / armRealRangeArmor * 0.15f));
            float realRangeArmor = armRealRangeArmor * realRangeArmorFactor; //实际破甲杀伤
            Debug.Log("实际普通杀伤：" + realRangeNormal + "  实际破甲杀伤：" + realRangeArmor + "  实际破甲杀伤系数：" +
                      realRangeArmorFactor);

            //计算实际攻击伤害
            int totalDamage = (int)(successAttackNum * (realRangeNormal + realRangeArmor)); //攻击产生的总伤害
            unitB.NowHp -= totalDamage; //计算剩余血量
            int theoryMaxNum = _armDataTypes[unitB.armId].totalTroops; //理论最大人数
            int theoryMinNum =
                (int)Math.Ceiling(theoryMaxNum * ((float)unitB.NowHp / _armDataTypes[unitB.armId].totalHp)); //理论最小人数
            float computeTroopsFactor = 0.7f; //剩余人数计算系数
            int theoryNowTroops = theoryMinNum + (int)((theoryMaxNum - theoryMinNum) * Math.Pow(unitB.NowHp /
                (float)_armDataTypes[unitB.armId].totalHp, computeTroopsFactor)); //剩余理论人数
            unitB.NowTroops = Math.Max(theoryMinNum, Math.Min(theoryMaxNum, theoryNowTroops)); //剩余实际人数
            Debug.Log("攻击产生的总伤害：" + totalDamage + "  理论最大人数：" + theoryMaxNum + "  理论最小人数：" + theoryMinNum +
                      "  剩余理论人数：" + theoryNowTroops);
        }

        /// <summary>
        /// 计算实际攻击能力，计算规则原则上先加后乘
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        private int RealAttack(UnitData unitData)
        {
            int correctAttack = _armDataTypes[unitData.armId].attack;
            if (unitData.isCharge)
            {
                correctAttack += _armDataTypes[unitData.armId].charge;
            }

            if (unitData.isStick)
            {
                correctAttack = (int)(correctAttack * 0.75f);
            }

            int realAttack = correctAttack; //修正后攻击能力
            if (_hasMorale)
            {
                realAttack = ComputeCorrectMorale(realAttack, unitData);
            }

            if (_hasFatigue)
            {
                realAttack = ComputeCorrectFatigue(realAttack, unitData);
            }

            return realAttack;
        }

        /// <summary>
        /// 计算实际近战防御能力，计算规则原则上先加后乘
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        private int RealDefenseMelee(UnitData unitData)
        {
            int correctDefenseMelee = _armDataTypes[unitData.armId].defenseMelee;
            if (unitData.isStick)
            {
                correctDefenseMelee = (int)(correctDefenseMelee * 1.4f);
            }

            int realDefenseMelee = correctDefenseMelee; //修正后防御能力
            if (_hasMorale)
            {
                realDefenseMelee = ComputeCorrectMorale(realDefenseMelee, unitData);
            }

            if (_hasFatigue)
            {
                realDefenseMelee = ComputeCorrectFatigue(realDefenseMelee, unitData);
            }

            return realDefenseMelee;
        }

        /// <summary>
        /// 计算实际破甲杀伤，计算规则原则上先加后乘
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        private int RealMeleeArmor(UnitData unitData)
        {
            int correctMeleeArmor = _armDataTypes[unitData.armId].meleeArmor;
            if (unitData.isCharge)
            {
                correctMeleeArmor += (int)(_armDataTypes[unitData.armId].charge * 0.1f);
            }

            int realMeleeArmor = correctMeleeArmor;
            return realMeleeArmor;
        }

        /// <summary>
        /// 计算实际射击精度，计算规则原则上先加后乘
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        private int RealAccuracy(UnitData unitData)
        {
            int correctAccuracy = _armDataTypes[unitData.armId].accuracy;
            int realAccuracy = correctAccuracy;
            if (_hasMorale)
            {
                realAccuracy = ComputeCorrectMorale(realAccuracy, unitData);
            }

            if (_hasFatigue)
            {
                realAccuracy = ComputeCorrectFatigue(realAccuracy, unitData);
            }

            return realAccuracy;
        }

        /// <summary>
        /// 计算实际远程防御能力
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        private int RealDefenseRange(UnitData unitData)
        {
            int correctDefenseRange = _armDataTypes[unitData.armId].defenseRange;
            int realDefenseRange = correctDefenseRange;
            if (_hasMorale)
            {
                realDefenseRange = ComputeCorrectMorale(realDefenseRange, unitData);
            }

            if (_hasFatigue)
            {
                realDefenseRange = ComputeCorrectFatigue(realDefenseRange, unitData);
            }

            return realDefenseRange;
        }

        /// <summary>
        /// 计算作战意志修正
        /// </summary>
        private int ComputeCorrectMorale(int value, UnitData unitData)
        {
            return (int)(value * (1 - (_armDataTypes[unitData.armId].maximumMorale - unitData.NowMorale) /
                (float)_armDataTypes[unitData.armId].maximumMorale * 0.2f));
        }

        /// <summary>
        /// 计算疲劳值修正
        /// </summary>
        private int ComputeCorrectFatigue(int value, UnitData unitData)
        {
            return (int)(value * (1 - unitData.NowFatigue /
                (float)_armDataTypes[unitData.armId].maximumFatigue * 0.5f));
        }

        /// <summary>
        /// 计算实际远程破甲杀伤
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        private int RealRangeArmor(UnitData unitData)
        {
            int correctRangeArmor = _armDataTypes[unitData.armId].rangeArmor;
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
        /// 计算受到攻击损失的作战意志
        /// </summary>
        /// <param name="lossTroops"></param>
        /// <param name="unitData"></param>
        private void AttackChangeMorale(int lossTroops, UnitData unitData)
        {
            int afterAttackLossTroops = lossTroops + unitData.NowTroops; //计算损失之前单位的人数
            float moraleRatio = lossTroops / (float)afterAttackLossTroops * 2; //计算损失比例参数
            float moraleLossRatio = moraleRatio * moraleRatio; //计算作战意志损失比例
            unitData.NowMorale -= (int)(moraleLossRatio * INIT_MORALE);
        }

        /// <summary>
        /// 计算疲劳值对作战意志的影响
        /// </summary>
        /// <param name="unitData"></param>
        private void FatigueChangeMorale(UnitData unitData)
        {
            float fatigueRatio =
                unitData.NowFatigue / (float)_armDataTypes[unitData.armId].maximumFatigue - 0.5f; //计算疲劳值影响参数
            float moraleLossRatio = Math.Max(0, fatigueRatio * fatigueRatio);
            unitData.NowMorale -= (int)(moraleLossRatio * INIT_MORALE);
        }

        /// <summary>
        /// 周围单位崩溃影响作战意志
        /// </summary>
        /// <param name="unitData"></param>
        /// <param name="isOur">是否是友方的单位</param>
        private void AroundUnitCollapseChangeMorale(UnitData unitData, bool isOur)
        {
            unitData.NowMorale -= (int)(0.2f * INIT_MORALE);
        }

        /// <summary>
        /// 己方将领阵亡影响作战意志
        /// </summary>
        /// <param name="unitData"></param>
        private void OurGeneralDieChangeMorale(UnitData unitData)
        {
            unitData.NowMorale -= (int)(0.3f * INIT_MORALE);
        }

        /// <summary>
        /// 敌方将领阵亡影响作战意志
        /// </summary>
        /// <param name="unitData"></param>
        private void EnemyGeneralDieChangeMorale(UnitData unitData)
        {
            unitData.NowMorale += (int)(0.2f * INIT_MORALE);
        }

        /// <summary>
        /// 被包围影响作战意志
        /// </summary>
        /// <param name="surroundRatio">被包围比例（0.33-1，即从两面被围到六面全被围）</param>
        /// <param name="unitData"></param>
        private void SurroundChangeMorale(float surroundRatio, UnitData unitData)
        {
            unitData.NowMorale -= (int)(0.25f * surroundRatio * INIT_MORALE);
        }

        /// <summary>
        /// 疲劳值改变
        /// </summary>
        private void ChangeFatigue()
        {
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
                { nameof(ArmDataType.attackRange), "攻击范围" },
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