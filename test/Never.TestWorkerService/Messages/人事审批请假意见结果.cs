﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Never.WorkFlow.Test.Messages
{
    public class 人事审批请假意见结果 : IWorkStepMessage
    {
        public Guid TaskId { get; set; }

        public int AttachState { get; set; }
    }
}