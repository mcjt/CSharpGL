﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpGL
{
    public sealed class GLZeroIndexNode : GLIndexNode
    {
        private static readonly Type type = typeof(GLZeroIndexNode);
        internal override Type ThisTypeCache
        {
            get { return type; }
        }
    }
}
