using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Components;
using OrbitCommander.Core;
using OrbitCommander.Particles;
using UILib.Content;
using System.Diagnostics;

namespace OrbitCommander.Entities;
public class Pickup : Entity, IData
{
    Texture2D IData.Texture => itemData.RealSprite;
    Color IData.Color => itemData.Color;
    public Items Type => itemData.Type;
    protected ItemData itemData;
    public Window Tooltip { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.WideButton));
    public string Name => itemData.Name;
    public int ID => itemData.ID;
    private Decal textbox;
    public Pickup(ItemData _itemData, Vector2 _position, Vector2 _velocity, float _angularVelocity, int _health = 10)
        : base(_position, _velocity, 0, _angularVelocity)
    {
        AddComponent(new Sprite(this, Color.Cyan) { Texture = _itemData.VirtualSprite });
        AddComponent(new Stealth(this) { StealthAbility = 0, SensingAbility = 0 });
        itemData = _itemData;
        AddComponent(new Friendly(this) { Team = Team.Friendly });
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        textbox = new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, _itemData.TextColor, 5f);
        Tooltip.AddWidget(textbox);
        AddComponent(new Health(this, _health));
        AddComponent(new Statuses(this));
        AddTag(Tags.IsImportant);
        AddComponent(new Collide(this, delegate (int _damage, bool _ignoreImmunity)
        {
            if (_damage <= 0)
            {
                return 0;
            }
            if (InvincibilityCooldown > 0 && !_ignoreImmunity)
            {
                InvincibilityCooldown = 0;
                return 0;
            }
            Health -= _damage;
            if (!_ignoreImmunity)
            {
                InvincibilityCooldown = 1;
            }
            if (Health <= 0)
            {
                isExpired = true;
            }
            SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            Engine.ShakeScreen(10 / ((Position - Engine.Camera.Position).Length() + 150));
            return _damage;
        }));
        InvincibilityCooldown = 5;
    }
    public Pickup(ItemData _itemData, List<string> _disassembly, LoadLogger _logger)
        : base(default, default, 0, 0)
    {
        AddComponent(new Sprite(this, Color.Cyan) { Texture = _itemData.VirtualSprite });
        itemData = _itemData;
        AddComponent(new Friendly(this) { Team = Team.Friendly });
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        textbox = new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, _itemData.TextColor, 5f);
        Tooltip.AddWidget(textbox);
        if(!int.TryParse(_disassembly[1], out int _health))
        {
            _health = 10;
        }
        AddComponent(new Health(this, _health));
        AddTag(Tags.IsImportant);
        AddComponent(new Collide(this, delegate (int _damage, bool _ignoreImmunity)
        {
            if (_damage <= 0)
            {
                return 0;
            }
            if (InvincibilityCooldown > 0 && !_ignoreImmunity)
            {
                InvincibilityCooldown = 0;
                return 0;
            }
            Health -= _damage;
            if (!_ignoreImmunity)
            {
                InvincibilityCooldown = 1;
            }
            if (Health <= 0)
            {
                isExpired = true;
            }
            SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            Engine.ShakeScreen(10 / ((Position - Engine.Camera.Position).Length() + 150));
            return _damage;
        }));
        InvincibilityCooldown = 5;
    }
    public void Parse(List<string> _disassembly, LoadLogger _logger)
    {
        _logger.Try(delegate 
        {
            int val = int.Parse(_disassembly[1]);
            Health = val;
        }, 1);
    }
    public override void Update()
    {
        int index = Player.leashedMaterials.IndexOf(this);
        if (index == -1)
        {
            if (Player.IsFriendly(this) && Vector2.DistanceSquared(Player.Position, Position) < 1375 && Player.leashedMaterials.Count < 3 && Player.canGatherResources)
            {
                Player.leashedMaterials.Add(this);
                if (Player.leashedMaterials.Count < 3)
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
                }
                else
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
                }
            }
            Velocity *= 1 - Engine.DeltaSeconds * 2;
        }
        else
        {
            Entity parent;
            if (index == 0)
            {
                parent = Player;
            }
            else
            {
                parent = Player.leashedMaterials[index - 1];
            }
            var relativePos = Vector2.Normalize(parent.Position - Position);
            Velocity += (parent.Position - relativePos * 20 - Position) * Engine.DeltaSeconds * 0.4f;
            float offset = Util.FIED(0.05f);
            Velocity = parent.Velocity * (1 - offset) + Velocity * offset;
        }
        base.Update();
    }
    public virtual string SerializeAttributes()
    {
        return $"{Health}";
    }
    public virtual string Serialize()
    {
        return $"{{{Type},{Health}}}";
    }
    IEnumerable<int> CryoBarricade()
    {
        float cooldown = 0;
        Entity nearestEnemy;
        Transform.IsImmovable = true;
        while (true)
        {
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            Angle = MathF.Atan2(Position.X, -Position.Y);
            nearestEnemy = Engine.SaveGame.CurrentMission.NearestEnemy(NewEnemy(Position, Vector2.Zero, 0, 0, null, Team));
            if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.Position, Position) < 300)
            {
                var dir = Vector2.Normalize(nearestEnemy.Position - Position);
                Engine.SaveGame.CurrentMission.Add(NewPulseShot(Position, dir * 10, MathF.Atan2(dir.X, -dir.Y), 0, Team, 5, true));
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                cooldown = 1.5f;
            }
            foreach (var enemy in Engine.SaveGame.CurrentMission.enemies)
            {
                float distSqr = Vector2.DistanceSquared(enemy.Position, Position);
                if (distSqr < 3600)
                {
                    enemy.ApplyWork(-0.5f);
                }
                if (IsFriendly(enemy))
                {
                    continue;
                }
                if (distSqr < 100000)
                {
                    Vector2 dir = (enemy.Position - Position) / (distSqr + 50) * 1000 * Engine.DeltaSeconds;
                    float lerp = MaxHealth / 2 / (enemy.MaxHealth + MaxHealth / 2);
                    Velocity += dir * lerp;
                    enemy.Velocity -= dir * (1 - lerp);
                }
                if (distSqr < (ColliderRadius + enemy.ColliderRadius) * (ColliderRadius + enemy.ColliderRadius) && enemy.HasComponent<Health>())
                {
                    //Mace sticks to enemies and has mass equal to maximum health div by 2
                    float lerp = MaxHealth / 2 / (enemy.MaxHealth + MaxHealth / 2);
                    Velocity = Velocity * (1 - lerp) + enemy.Velocity * lerp;
                    enemy.Velocity = Velocity;
                }
            }
            if (Vector2.DistanceSquared(Position, Player.Position) < 1600)
            {
                Player.ApplyWork(-0.33f);
            }
            GetComponent<FollowEmitter>().ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
            yield return 0;
        }
    }
    public static Pickup NewCryoBarricade(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, Team _team = Team.Friendly)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.CryoBarricade], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.CryoBarricade].Integrity);
        construct.AddComponent(new Behaviour().AddBehaviour(construct.CryoBarricade()));
        construct.AddComponent<Smelt>(new Smelt() { Value = 1 });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.Team = _team;
        return construct;
    }
    IEnumerable<int> Trap()
    {
        float cooldown = 0;
        Entity nearestEnemy;
        Transform.IsImmovable = true;
        while (true)
        {
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            nearestEnemy = Engine.SaveGame.CurrentMission.NearestEnemy(NewEnemy(Position, Vector2.Zero, 0, 0, null, Team));
            if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.Position, Position) < 800)
            {
                var dir = Vector2.Normalize(nearestEnemy.Position - Position);
                var enemies = Engine.SaveGame.CurrentMission.Hitscan(Position, dir, 800, true, out Vector2 _end, Friendly.Blacklist(Team));
                foreach (var enemy in enemies)
                {
                    enemy.Collide(10);
                }
                for (int i = 0; i < (_end - Position).Length() / 4; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position + dir * 4 * i, Vector2.Zero, Util.ToAngle(dir), 0, Color.Red, Color.Transparent));
                }
                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                cooldown = 0.75f;
            }
            GetComponent<FollowEmitter>().ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
            yield return 0;
        }
    }
    public static Pickup NewTrap(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, Team _team = Team.Friendly)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Trap], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.Trap].Integrity);
        construct.AddComponent(new Behaviour().AddBehaviour(construct.Trap()));
        construct.AddComponent(new FollowEmitter(construct) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), _position, 300, new Color(255, 0, 0)) });
        construct.AddComponent<Smelt>(new Smelt() { Value = 1 });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.Team = _team;
        return construct;
    }
    IEnumerable<int> Bomb()
    {
        while (!isExpired)
        {
            yield return 0;
        }
        var tex = Assets.Get(Sprites.Explosion);
        ParticleManager.Add(new Particle(tex, 3, Position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
        Engine.SaveGame.CurrentMission.Explode(100, 100, Position);
        yield return 1;
    }
    public static Pickup NewBomb(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Bomb], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.Bomb].Integrity);
        construct.AddComponent(new Behaviour().AddBehaviour(construct.Bomb()));
        construct.AddComponent(new FollowEmitter(construct) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), _position, 100, new Color(255, 0, 0)) });
        construct.AddComponent<Smelt>(new Smelt() { Value = 1 });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.Team = Team.Dead; //Allows getting hit by any team
        return construct;
    }
    IEnumerable<int> Furnace()
    {
        float cooldown = 0;
        while (true)
        {
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            Velocity *= Util.FIED(0.2f);
            foreach (var enemy in Engine.SaveGame.CurrentMission.Entities)
            {
                float distSqr = Vector2.DistanceSquared(enemy.Position, Position);
                if (distSqr < 3600)
                {
                    enemy.ApplyWork(0.5f);
                }
            }
            if (Vector2.DistanceSquared(Position, Player.Position) < 2000)
            {
                Player.ApplyWork(0.33f);
            }
            Vector2 offset = Util.RotateVector2(new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 5, Angle);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position + offset, Velocity, Angle, 0, Color.Orange, Color.Transparent));
            var nearestPickup = Engine.SaveGame.CurrentMission.NearestItem(this, true);
            if (nearestPickup == null)
            {
                break;
            }
            Vector2 relativePosition = nearestPickup.Position - Position;
            if (relativePosition.X < 7 && relativePosition.X > -7 && relativePosition.Y < 7 && relativePosition.Y > -7)
            {
                nearestPickup.Position = Position;
                if (Player.leashedMaterials.Contains(nearestPickup as Pickup))
                {
                    Player.leashedMaterials.Remove(nearestPickup as Pickup);
                }
                cooldown += Engine.DeltaSeconds * 2;
                ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position + Util.RotateVector2(new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 5, Angle),
                    Velocity, Angle, 0, Color.Orange, Color.Transparent));
                if (cooldown > 15)
                {
                    nearestPickup.isExpired = true;
                    cooldown = 0;
                    Engine.SaveGame.Scrap += nearestPickup.GetComponent<Smelt>().Value;
                    SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
                }
            }
            yield return 0;
        }
    }
    public static Pickup NewFurnace(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, Team _team = Team.Friendly)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Furnace], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.Furnace].Integrity);
        construct.AddComponent(new Behaviour().AddBehaviour(construct.Furnace()));
        construct.AddComponent(new FollowEmitter(construct) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), _position, 100, new Color(255, 0, 0)) });
        construct.AddComponent(new Smelt() { Value = 1 });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.Team = _team;
        return construct;
    }
    public static Pickup NewSpecializedParts(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, Team _team = Team.Friendly)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.SpecializedParts], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.SpecializedParts].Integrity)
        {
            Angle = _angle,
            StealthAbility = _stealth,
            Team = _team
        };
        construct.AddTag(Tags.IsSpecialized);
        return construct;
    }
    IEnumerable<int> FaradayShield()
    {
        while (true)
        {
            foreach (var enemy in Engine.SaveGame.CurrentMission.enemies.Where(x => IsFriendly(x) && x.HasComponent<Statuses>()))
            {
                float distSqr = Vector2.DistanceSquared(enemy.Position, Position);
                if(distSqr < 22500)
                {
                    enemy.Statuses.ApplyStatus(new FleetingDefense());
                }
            }
            if (Vector2.DistanceSquared(Position, Player.Position) < 22500)
            {
                Player.Statuses.ApplyStatus(new FleetingDefense());
            }
            yield return 0;
        }
    }
    public static Pickup NewFaradayShield(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, Team _team = Team.Friendly)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.FaradayShield], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.FaradayShield].Integrity)
        {
            Angle = _angle,
            StealthAbility = _stealth,
            Team = _team
        };
        construct.AddComponent(new Smelt() { Value = 1 });
        construct.AddComponent(new Behaviour().AddBehaviour(construct.FaradayShield()));
        return construct;
    }
}
public class ItemData(Sprites _realSprite, Sprites _virtualSprite, string _name, int _id, Color _color, Color? _textColor = null, int _integrity = 3)
{
    public Texture2D RealSprite { get; } = Assets.Get(_realSprite);
    public Texture2D VirtualSprite { get; } = Assets.Get(_virtualSprite);
    public string Name { get; } = _name;
    public int ID { get; } = _id;
    public Color Color { get; } = _color;
    public Color TextColor { get; } = _textColor ?? Color.White;
    public Items Type { get; } = Items.Scrap;
    public int Integrity { get; } = _integrity;
}
public enum Items
{
    Scrap,
    CryoBarricade,
    Trap,
    Bomb,
    SpecializedParts,
    Furnace,
    FaradayShield
}
