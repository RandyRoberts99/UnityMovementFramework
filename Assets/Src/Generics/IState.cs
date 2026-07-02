using UnityEngine;

namespace Radknee.Generics
{
    public interface IState
    {
        void Start();
        void Process();
        void End();
        IState Switch();
    }
}