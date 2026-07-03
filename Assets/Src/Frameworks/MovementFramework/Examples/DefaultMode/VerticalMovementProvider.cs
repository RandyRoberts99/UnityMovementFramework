using Radknee.Generics;
using System.Collections.Generic;
using UnityEngine;

namespace Radknee.MovementFramework.Examples
{
    public class VerticalMovementProvider : MovementProvider
    {
        public VerticalMovementProvider(List<IState> states)
        {
            States = states;
            CurrentState = RequestState<GroundedState>();
            CurrentState.Start();
        }
    }
}