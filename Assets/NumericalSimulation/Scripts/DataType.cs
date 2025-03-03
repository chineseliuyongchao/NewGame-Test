using System;
using Utils.Script;

namespace NumericalSimulation.Scripts
{
    /// <summary>
    /// 兵种数据类型
    /// </summary>
    [Serializable]
    public class ArmDataType : BaseJsonData
    {
        /// <summary>
        /// 兵种名称
        /// </summary>
        public string unitName;

        /// <summary>
        /// 总血量
        /// </summary>
        public int totalHp;

        /// <summary>
        /// 总人数
        /// </summary>
        public int totalTroops;

        /// <summary>
        /// 攻击能力
        /// </summary>
        public int attack;

        /// <summary>
        /// 攻击范围
        /// </summary>
        public int attackRange;

        /// <summary>
        /// 冲锋加成
        /// </summary>
        public int charge;

        /// <summary>
        /// 防御能力（近战）
        /// </summary>
        public int defenseMelee;

        /// <summary>
        /// 防御能力（远程）
        /// </summary>
        public int defenseRange;

        /// <summary>
        /// 近战杀伤（普通）
        /// </summary>
        public int meleeNormal;

        /// <summary>
        /// 近战杀伤（破甲）
        /// </summary>
        public int meleeArmor;

        /// <summary>
        /// 护甲强度
        /// </summary>
        public int armor;

        /// <summary>
        /// 移动能力
        /// </summary>
        public int mobility;

        /// <summary>
        /// 视野
        /// </summary>
        public int sight;

        /// <summary>
        /// 隐蔽
        /// </summary>
        public int stealth;

        /// <summary>
        /// 弹药量
        /// </summary>
        public int ammo;

        /// <summary>
        /// 射程
        /// </summary>
        public int range;

        /// <summary>
        /// 装填速度
        /// </summary>
        public int reload;

        /// <summary>
        /// 精度
        /// </summary>
        public int accuracy;

        /// <summary>
        /// 远程杀伤（普通）
        /// </summary>
        public int rangeNormal;

        /// <summary>
        /// 远程杀伤（破甲）
        /// </summary>
        public int rangeArmor;

        /// <summary>
        /// 最高作战意志
        /// </summary>
        public int maximumMorale;

        /// <summary>
        /// 疲劳值上限
        /// </summary>
        public int maximumFatigue;

        /// <summary>
        /// 价格
        /// </summary>
        public int cost;
    }

    /// <summary>
    /// 在攻击模拟时记录一个单位的实时属性
    /// </summary>
    public class UnitData
    {
        public ArmDataType armDataType;

        /// <summary>
        /// 兵种id
        /// </summary>
        public int armId;

        private int _nowHp;

        /// <summary>
        /// 当前血量
        /// </summary>
        public int NowHp
        {
            get => _nowHp;
            set
            {
                _nowHp = value;
                if (_nowHp < 0)
                {
                    _nowHp = 0;
                }
            }
        }

        private int _nowTroops;

        /// <summary>
        /// 当前人数
        /// </summary>
        public int NowTroops
        {
            get => _nowTroops;
            set
            {
                _nowTroops = value;
                if (_nowTroops < 0)
                {
                    _nowTroops = 0;
                }
            }
        }

        private int _nowAmmo;

        /// <summary>
        /// 当前弹药量
        /// </summary>
        public int NowAmmo
        {
            get => _nowAmmo;
            set => _nowAmmo = value;
        }

        private int _nowMorale;

        /// <summary>
        /// 当前作战意志
        /// </summary>
        public int NowMorale
        {
            get => _nowMorale;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                _nowMorale = value;
            }
        }

        private int _nowFatigue;

        /// <summary>
        /// 当前疲劳值
        /// </summary>
        public int NowFatigue
        {
            get => _nowFatigue;
            set
            {
                if (value > armDataType.maximumFatigue)
                {
                    value = armDataType.maximumFatigue;
                }

                _nowFatigue = value;
            }
        }

        /// <summary>
        /// 单位是否在冲锋（模拟时才会用到）
        /// </summary>
        public bool isCharge;

        /// <summary>
        /// 单位是否在坚守
        /// </summary>
        public bool isStick;

        public UnitData(UnitData unitData)
        {
            armDataType = unitData.armDataType;
            armId = unitData.armId;
            _nowHp = unitData._nowHp;
            _nowTroops = unitData._nowTroops;
            _nowAmmo = unitData._nowAmmo;
            _nowMorale = unitData._nowMorale;
            _nowFatigue = unitData._nowFatigue;
            isCharge = unitData.isCharge;
            isStick = unitData.isStick;
        }

        public UnitData(ArmDataType armDataType, int id)
        {
            this.armDataType = armDataType;
            armId = id;
            _nowHp = armDataType.totalHp;
            _nowTroops = armDataType.totalTroops;
            _nowAmmo = armDataType.ammo;
            _nowMorale = armDataType.maximumMorale;
            _nowFatigue = 0;
            isCharge = false;
            isStick = false;
        }

        public void Reset(UnitData data)
        {
            armId = data.armId;
            _nowHp = data._nowHp;
            _nowTroops = data._nowTroops;
            _nowAmmo = data._nowAmmo;
            _nowMorale = data._nowMorale;
            _nowFatigue = 0;
        }
    }

    /// <summary>
    /// 攻击形式
    /// </summary>
    public enum AttackFormType
    {
        /// <summary>
        /// 单向攻击：只有队伍a会攻击，b不会反击
        /// </summary>
        ONE_WAY_ATTACK,

        /// <summary>
        /// 互相攻击：a会攻击，b也会反击
        /// </summary>
        MUTUAL_ATTACK,

        /// <summary>
        /// 互相进攻：a会进攻，b也会进攻，同时ab都会反击
        /// </summary>
        MUTUAL_OFFENSE,

        /// <summary>
        /// 单向射击
        /// </summary>
        ONE_WAY_SHOOTING,

        /// <summary>
        /// 互相射击
        /// </summary>
        MUTUAL_SHOOTING
    }
}