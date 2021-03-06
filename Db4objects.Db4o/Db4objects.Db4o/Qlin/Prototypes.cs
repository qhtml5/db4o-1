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
using System.Collections;
using Db4objects.Db4o.Foundation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Core;
using Db4objects.Db4o.Reflect.Generic;
using Sharpen;

namespace Db4objects.Db4o.Qlin
{
    /// <summary>creates prototype objects for classes.</summary>
    /// <remarks>
    ///     creates prototype objects for classes. Each field on prototype objects is set
    ///     to a newly created object or primitive that can be identified either by it's
    ///     identity or by an int ID that is generated by the system. Creation of fields
    ///     is recursed to the depth specified in the constructor.<br />
    ///     <br />
    ///     Allows analyzing expressions called on prototype objects to find the
    ///     underlying field that delivers the return value of the expression. Passed
    ///     expressions should not have side effects on objects, otherwise the
    ///     "prototype world" will no longer work.<br />
    ///     <br />
    ///     We plan to supply an ImmutableFieldClassLoader to instrument the code to
    ///     throw on every modification. This ClassLoader could also supply information
    ///     about all the method calls involved.<br />
    ///     <br />
    ///     For now our approach only works if expressions are directly backed by a
    ///     single field.<br />
    ///     <br />
    ///     We were inspired for this approach when we saw that Thomas Mueller managed to
    ///     map expressions to fields for his JaQu query interface, Kudos!
    ///     http://www.h2database.com/html/jaqu.html<br />
    ///     <br />
    ///     We took the idea a bit further and made it work for all primitives except for
    ///     boolean and we plan to also get deeper expressions, collections and
    ///     interfaces working nicely.
    /// </remarks>
    public class Prototypes
    {
        private static Hashtable4 _integerConverters;
        private static readonly string StringIdentifier = "QLinIdentity";
        private readonly bool _ignoreTransient;
        private readonly Hashtable4 _prototypes = new Hashtable4();
        private readonly int _recursionDepth;
        private readonly IReflector _reflector;

        public Prototypes(IReflector reflector, int recursionDepth, bool ignoreTransient)
        {
            _reflector = reflector;
            _recursionDepth = recursionDepth;
            _ignoreTransient = ignoreTransient;
        }

        public Prototypes() : this(DefaultReflector(), 5, false)
        {
        }

        /// <summary>returns a prototype object for a specific class.</summary>
        /// <remarks>returns a prototype object for a specific class.</remarks>
        public virtual object PrototypeForClass(Type clazz)
        {
            if (clazz == null)
            {
                throw new PrototypesException("Class can not be null");
            }
            var claxx = _reflector.ForClass(clazz);
            if (claxx == null)
            {
                throw new PrototypesException("Not found in the reflector: " + clazz);
            }
            var className = claxx.GetName();
            var prototype = (Prototype) _prototypes.Get(className);
            if (prototype != null)
            {
                return prototype.Object();
            }
            prototype = new Prototype(this, claxx);
            _prototypes.Put(className, prototype);
            return prototype.Object();
        }

        /// <summary>
        ///     analyzes the passed expression and tries to find the path to the
        ///     backing field that is accessed.
        /// </summary>
        /// <remarks>
        ///     analyzes the passed expression and tries to find the path to the
        ///     backing field that is accessed.
        /// </remarks>
        public virtual IEnumerator BackingFieldPath(Type clazz, object expression)
        {
            return BackingFieldPath(_reflector.ForClass(clazz), expression);
        }

        /// <summary>
        ///     analyzes the passed expression and tries to find the path to the
        ///     backing field that is accessed.
        /// </summary>
        /// <remarks>
        ///     analyzes the passed expression and tries to find the path to the
        ///     backing field that is accessed.
        /// </remarks>
        public virtual IEnumerator BackingFieldPath(IReflectClass claxx, object expression
            )
        {
            return BackingFieldPath(claxx.GetName(), expression);
        }

        /// <summary>
        ///     analyzes the passed expression and tries to find the path to the
        ///     backing field that is accessed.
        /// </summary>
        /// <remarks>
        ///     analyzes the passed expression and tries to find the path to the
        ///     backing field that is accessed.
        /// </remarks>
        public virtual IEnumerator BackingFieldPath(string className, object expression)
        {
            var prototype = (Prototype) _prototypes.Get(className);
            if (prototype == null)
            {
                return null;
            }
            return prototype.BackingFieldPath(_reflector, expression);
        }

        private static IntegerConverter IntegerConverterforClassName(IReflector
            reflector, string className)
        {
            if (_integerConverters == null)
            {
                _integerConverters = new Hashtable4();
                IntegerConverter[] converters =
                {
                    new
                        _IntegerConverter_211(),
                    new _IntegerConverter_215(), new _IntegerConverter_219(
                        ),
                    new _IntegerConverter_223(), new _IntegerConverter_227(), new _IntegerConverter_231
                        (),
                    new _IntegerConverter_235(), new _IntegerConverter_239()
                };
                for (var converterIndex = 0; converterIndex < converters.Length; ++converterIndex)
                {
                    var converter = converters[converterIndex];
                    _integerConverters.Put(converter.PrimitiveName(), converter);
                    if (!converter.PrimitiveName().Equals(converter.WrapperName(reflector)))
                    {
                        _integerConverters.Put(converter.WrapperName(reflector), converter);
                    }
                }
            }
            return (IntegerConverter) _integerConverters.Get(className);
        }

        // Strings get prepended the following, so we can also use strings 
        // without restrictions in queries.
        public virtual IReflector Reflector()
        {
            return _reflector;
        }

        // We could always use this, but we want to make users of this class
        // aware that they have control over the reflector and that it is
        // important.
        public static IReflector DefaultReflector()
        {
            return new GenericReflector(Platform4.ReflectorForType(typeof (Prototypes)));
        }

        private static bool TrySetField(IReflectField field, object onObject, object value
            )
        {
            try
            {
                field.Set(onObject, value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private class Prototype
        {
            private readonly Prototypes _enclosing;
            private readonly IdentityHashtable4 _fieldsByIdentity = new IdentityHashtable4();
            private readonly Hashtable4 _fieldsByIntId = new Hashtable4();
            private readonly object _object;
            private int intIdGenerator;

            public Prototype(Prototypes _enclosing, IReflectClass claxx)
            {
                this._enclosing = _enclosing;
                _object = claxx.NewInstance();
                if (_object == null)
                {
                    throw new PrototypesException("Prototype could not be created for class " + claxx
                        .GetName());
                }
                Analyze(_object, claxx, this._enclosing._recursionDepth, null);
            }

            private void Analyze(object @object, IReflectClass claxx, int depth, List4 parentPath
                )
            {
                if (depth < 0)
                {
                    return;
                }
                ReflectorUtils.ForEachField(claxx, new _IProcedure4_130(this, parentPath, claxx,
                    @object, depth));
            }

            public virtual object Object()
            {
                return _object;
            }

            public virtual IEnumerator BackingFieldPath(IReflector reflector, object expression
                )
            {
                if (expression == null)
                {
                    return null;
                }
                var claxx = reflector.ForObject(expression);
                if (claxx == null)
                {
                    return null;
                }
                var converter = IntegerConverterforClassName(reflector
                    , claxx.GetName());
                if (converter != null)
                {
                    var entry = (Pair) _fieldsByIntId.Get(converter.ToInteger(expression));
                    if (entry == null)
                    {
                        return null;
                    }
                    if (entry.first.Equals(expression))
                    {
                        return AsIterator((List4) entry.second);
                    }
                    return null;
                }
                if (claxx.IsPrimitive())
                {
                    return null;
                }
                return AsIterator((List4) _fieldsByIdentity.Get(expression));
            }

            private IEnumerator AsIterator(List4 lastElement)
            {
                return Iterators.Revert(Iterators.Map(Iterators.Iterate(lastElement), new _IFunction4_198
                    ()));
            }

            private sealed class _IProcedure4_130 : IProcedure4
            {
                private readonly Prototype _enclosing;
                private readonly IReflectClass claxx;
                private readonly int depth;
                private readonly object @object;
                private readonly List4 parentPath;

                public _IProcedure4_130(Prototype _enclosing, List4 parentPath, IReflectClass claxx
                    , object @object, int depth)
                {
                    this._enclosing = _enclosing;
                    this.parentPath = parentPath;
                    this.claxx = claxx;
                    this.@object = @object;
                    this.depth = depth;
                }

                public void Apply(object field)
                {
                    if (((IReflectField) field).IsStatic())
                    {
                        return;
                    }
                    if (_enclosing._enclosing._ignoreTransient && ((IReflectField) field).IsTransient
                        ())
                    {
                        return;
                    }
                    var fieldType = ((IReflectField) field).GetFieldType();
                    var path = new List4(parentPath, ((IReflectField) field));
                    var converter = IntegerConverterforClassName(claxx
                        .Reflector(), fieldType.GetName());
                    if (converter != null)
                    {
                        var id = ++_enclosing.intIdGenerator;
                        var integerRepresentation = converter.FromInteger(id);
                        if (!TrySetField(((IReflectField) field), @object, integerRepresentation
                            ))
                        {
                            return;
                        }
                        _enclosing._fieldsByIntId.Put(id, new Pair(integerRepresentation, path));
                        return;
                    }
                    if (!fieldType.IsPrimitive())
                    {
                        var identityInstance = fieldType.NewInstance();
                        if (identityInstance == null)
                        {
                            return;
                        }
                        if (!TrySetField(((IReflectField) field), @object, identityInstance))
                        {
                            return;
                        }
                        _enclosing._fieldsByIdentity.Put(identityInstance, path);
                        _enclosing.Analyze(identityInstance, claxx, depth - 1, path);
                    }
                }
            }

            private sealed class _IFunction4_198 : IFunction4
            {
                public object Apply(object field)
                {
                    return ((IReflectField) field).GetName();
                }
            }
        }

        private sealed class _IntegerConverter_211 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (int).FullName;
            }

            public override object FromInteger(int i)
            {
                return i;
            }
        }

        private sealed class _IntegerConverter_215 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (long).FullName;
            }

            public override object FromInteger(int i)
            {
                return Convert.ToInt64(i);
            }
        }

        private sealed class _IntegerConverter_219 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (double).FullName;
            }

            public override object FromInteger(int i)
            {
                return Convert.ToDouble(i);
            }
        }

        private sealed class _IntegerConverter_223 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (float).FullName;
            }

            public override object FromInteger(int i)
            {
                return Convert.ToSingle(i);
            }
        }

        private sealed class _IntegerConverter_227 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (byte).FullName;
            }

            public override object FromInteger(int i)
            {
                return (byte) i;
            }
        }

        private sealed class _IntegerConverter_231 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (char).FullName;
            }

            public override object FromInteger(int i)
            {
                return (char) i;
            }
        }

        private sealed class _IntegerConverter_235 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (short).FullName;
            }

            public override object FromInteger(int i)
            {
                return (short) i;
            }
        }

        private sealed class _IntegerConverter_239 : IntegerConverter
        {
            public override string PrimitiveName()
            {
                return typeof (string).FullName;
            }

            public override object FromInteger(int i)
            {
                return StringIdentifier + i;
            }

            public override int ToInteger(object obj)
            {
                if (!(obj is string))
                {
                    return -1;
                }
                var str = (string) obj;
                if (str.Length < StringIdentifier.Length)
                {
                    return -1;
                }
                if (str.IndexOf(StringIdentifier) != 0)
                {
                    return -1;
                }
                return int.Parse(Runtime.Substring(str, StringIdentifier.Length
                    ));
            }
        }

        private abstract class IntegerConverter
        {
            public virtual string WrapperName(IReflector reflector)
            {
                return reflector.ForObject(FromInteger(1)).GetName();
            }

            public abstract string PrimitiveName();
            public abstract object FromInteger(int i);

            public virtual int ToInteger(object obj)
            {
                return int.Parse(obj.ToString());
            }
        }
    }
}