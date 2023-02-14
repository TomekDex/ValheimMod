using System.Collections.Generic;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class ContainerIdDistanceComparer : IComparer<ContainerIdDistance>
    {
        public int Compare(ContainerIdDistance x, ContainerIdDistance y)
        {
            if (x.Equals(y))
                return 0;
            int value = x.Distance.CompareTo(y.Distance);
            if (value == 0)
                return 1;
            return x.Distance.CompareTo(y.Distance);
        }
    }

    public class ContainerIdDistance
    {
        public Vector3 LastPosition { get; }
        public Container Container { get; }
        public int Id { get; }
        public float Distance { get; }

        public ContainerIdDistance(Container container, float distance = default)
        {
            Container = container;
            Id = container.GetInstanceID();
            Distance = distance;
            LastPosition = new Vector3(container.transform.position.x, container.transform.position.y, container.transform.position.z);
        }

        public override int GetHashCode()
        {
            SortedSet<int> a = new SortedSet<int>();
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return Id.Equals(((ContainerIdDistance)obj).Id);
        }
    }
}
