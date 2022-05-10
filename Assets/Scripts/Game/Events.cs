using UnityEditor;
using UnityEngine.Events;

namespace Game { 

public class Events {

    public static UnityEvent<EntitySpawnEvent> ENTITY_SPAWN = new();
    public static UnityEvent<EntityDestroyEvent> ENTITY_DESTROY = new();
    public static UnityEvent<EntityDamageEvent> ENTITY_DAMAGE = new();


    public class EntitySpawnEvent {
        public readonly ForceEntity entity;

        public EntitySpawnEvent(ForceEntity entity) {
            this.entity = entity;
        }
    }
    
    public class EntityDestroyEvent {
        public readonly ForceEntity entity;

        public EntityDestroyEvent(ForceEntity entity) {
            this.entity = entity;
        }
    }

    public class EntityDamageEvent {
        public ForceEntity damager;
        public ForceEntity damaged;
        public float totalDamage;
        public float appliedDamage;
    }

}
}