﻿// Copyright © Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Seq.App.HttpRequest.Expressions.Ast;

namespace Seq.App.HttpRequest.Templates.Ast
{
    class Conditional : Template
    {
        public Expression Condition { get; }
        public Template Consequent { get; }
        public Template? Alternative { get; }

        public Conditional(Expression condition, Template consequent, Template? alternative)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Consequent = consequent ?? throw new ArgumentNullException(nameof(consequent));
            Alternative = alternative;
        }
    }
}
