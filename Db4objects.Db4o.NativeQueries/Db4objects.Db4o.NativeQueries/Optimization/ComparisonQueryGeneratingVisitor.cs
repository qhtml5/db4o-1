/* This file is part of the db4o object database http://www.db4o.com

Copyright (C) 2004 - 2011  Versant Corporation http://www.versant.com

db4o is free software; you can redistribute it and/or modify it under
the terms of version 3 of the GNU General Public License as published
by the Free Software Foundation.

db4o is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
for more details.

You should have received a copy of the GNU General Public License along
with this program.  If not, see http://www.gnu.org/licenses/. */

using System;
using System.Reflection;
using Db4objects.Db4o.Instrumentation.Api;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.NativeQueries.Expr.Cmp;
using Db4objects.Db4o.NativeQueries.Expr.Cmp.Operand;
using Sharpen;

namespace Db4objects.Db4o.NativeQueries.Optimization
{
    internal sealed class ComparisonQueryGeneratingVisitor : IComparisonOperandVisitor
    {
        private readonly INativeClassFactory _classSource;
        private readonly object _predicate;
        private readonly IReferenceResolver _resolver;
        private object _value;

        public ComparisonQueryGeneratingVisitor(object predicate, INativeClassFactory classSource
            , IReferenceResolver resolver)
        {
            _predicate = predicate;
            _classSource = classSource;
            _resolver = resolver;
        }

        public void Visit(ConstValue operand)
        {
            _value = operand.Value();
        }

        public void Visit(FieldValue operand)
        {
            operand.Parent().Accept(this);
            var clazz = ((operand.Parent() is StaticFieldRoot)
                ? (Type) _value
                : _value.GetType
                    ());
            try
            {
                var field = Reflection4.GetField(clazz, operand.FieldName());
                _value = field.GetValue(_value);
            }
            catch (Exception exc)
            {
                // arg is ignored for static
                Runtime.PrintStackTrace(exc);
            }
        }

        public void Visit(ArithmeticExpression operand)
        {
            operand.Left().Accept(this);
            var left = _value;
            operand.Right().Accept(this);
            var right = _value;
            switch (operand.Op().Id())
            {
                case ArithmeticOperator.AddId:
                {
                    _value = Add(left, right);
                    break;
                }

                case ArithmeticOperator.SubtractId:
                {
                    _value = Subtract(left, right);
                    break;
                }

                case ArithmeticOperator.MultiplyId:
                {
                    _value = Multiply(left, right);
                    break;
                }

                case ArithmeticOperator.DivideId:
                {
                    _value = Divide(left, right);
                    break;
                }

                case ArithmeticOperator.ModuloId:
                {
                    _value = Modulo(left, right);
                    break;
                }
            }
        }

        public void Visit(CandidateFieldRoot root)
        {
        }

        public void Visit(PredicateFieldRoot root)
        {
            _value = _predicate;
        }

        public void Visit(StaticFieldRoot root)
        {
            try
            {
                _value = _classSource.ForName(root.Type.Name);
            }
            catch (TypeLoadException e)
            {
                Runtime.PrintStackTrace(e);
            }
        }

        public void Visit(ArrayAccessValue operand)
        {
            operand.Parent().Accept(this);
            var parent = _value;
            operand.Index().Accept(this);
            var index = (int) _value;
            _value = Runtime.GetArrayValue(parent, index);
        }

        public void Visit(MethodCallValue operand)
        {
            operand.Parent().Accept(this);
            var receiver = _value;
            var method = _resolver.Resolve(operand.Method);
            try
            {
                _value = method.Invoke(IsStatic(method) ? null : receiver, Args(operand));
            }
            catch (Exception exc)
            {
                Runtime.PrintStackTrace(exc);
                _value = null;
            }
        }

        public object Value()
        {
            return _value;
        }

        internal object Add(object a, object b)
        {
            if (a is double || b is double)
            {
                return ((double) a) + ((double) b);
            }
            if (a is float || b is float)
            {
                return ((float) a) + ((float) b);
            }
            if (a is long || b is long)
            {
                return ((long) a) + ((long) b);
            }
            return ((int) a) + ((int) b);
        }

        internal object Subtract(object a, object b)
        {
            if (a is double || b is double)
            {
                return ((double) a) - ((double) b);
            }
            if (a is float || b is float)
            {
                return ((float) a) - ((float) b);
            }
            if (a is long || b is long)
            {
                return ((long) a) - ((long) b);
            }
            return ((int) a) - ((int) b);
        }

        internal object Multiply(object a, object b)
        {
            if (a is double || b is double)
            {
                return ((double) a)*((double) b);
            }
            if (a is float || b is float)
            {
                return ((float) a)*((float) b);
            }
            if (a is long || b is long)
            {
                return ((long) a)*((long) b);
            }
            return ((int) a)*((int) b);
        }

        internal object Divide(object a, object b)
        {
            if (a is double || b is double)
            {
                return ((double) a)/((double) b);
            }
            if (a is float || b is float)
            {
                return ((float) a)/((float) b);
            }
            if (a is long || b is long)
            {
                return ((long) a)/((long) b);
            }
            return ((int) a)/((int) b);
        }

        internal object Modulo(object a, object b)
        {
            if (a is double || b is double)
            {
                return ((double) a)%((double) b);
            }
            if (a is float || b is float)
            {
                return ((float) a)%((float) b);
            }
            if (a is long || b is long)
            {
                return ((long) a)%((long) b);
            }
            return ((int) a)%((int) b);
        }

        private object[] Args(MethodCallValue operand)
        {
            var args = operand.Args;
            var @params = new object[args.Length];
            for (var paramIdx = 0; paramIdx < args.Length; paramIdx++)
            {
                args[paramIdx].Accept(this);
                @params[paramIdx] = _value;
            }
            return @params;
        }

        private bool IsStatic(MethodInfo method)
        {
            return NativeQueriesPlatform.IsStatic(method);
        }
    }
}