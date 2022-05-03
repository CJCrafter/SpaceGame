using UnityEngine.Events;

namespace Game { 

public class Events {

    public static UnityEvent<ShipSpawnEvent> SHIP_SPAWN = new();
    public static UnityEvent<ShipDestroyEvent> SHIP_DESTROY = new();
    public static UnityEvent<ShipDamageEvent> SHIP_DAMAGE = new();


    public class ShipSpawnEvent {
        public readonly Ship ship;

        public ShipSpawnEvent(Ship ship) {
            this.ship = ship;
        }
    }

    public class ShipDestroyEvent {
        public readonly Ship ship;

        public ShipDestroyEvent(Ship ship) {
            this.ship = ship;
        }
    }

    public class ShipDamageEvent {
        public readonly Ship damager;
        public readonly Ship damaged;
        public float totalDamage;
        public float appliedDamage;
    }

}
}