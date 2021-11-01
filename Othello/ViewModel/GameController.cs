using Othello.ViewModel.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Othello.ViewModel
{
    public class GameController
    {
        private readonly GameParam param;
        
        public GameController()
        {
        }

        public GameController(GameParam param)
        {
            this.param = param;
        }
    }
}
