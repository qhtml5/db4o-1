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
using System.IO;

using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using Db4odoc.Tutorial;
using Db4odoc.Tutorial.F1;

namespace Db4odoc.Tutorial.F1.Chapter3
{
    public class OMEExample : Util
    {
        readonly static string YapFileName = Path.Combine(
                               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                               "ome.db4o");

        public static void Main(string[] args)
        {
            File.Delete(YapFileName);
            StorePilots();
        }

        public static void StorePilots()
        {
            using(IObjectContainer db = Db4oEmbedded.OpenFile(YapFileName))
            {
                Pilot pilot1 = new Pilot("Michael Schumacher", 100);
                db.Store(pilot1);
                Console.WriteLine("Stored {0}", pilot1);
                Pilot pilot2 = new Pilot("Rubens Barrichello", 99);
                db.Store(pilot2);
                Console.WriteLine("Stored {0}", pilot2);
            }
        }

    }
}
