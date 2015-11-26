﻿/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;

namespace RealmNet
{
    public class ObjectIdAttribute : Attribute
    {
    }

    public class IndexedAttribute : Attribute
    {
    }

    public class IgnoredAttribute : Attribute
    {
    }

    public class MapToAttribute : Attribute
    {
        public string Mapping { get; set; }
        public MapToAttribute(string mapping)
        {
            this.Mapping = mapping;
        }
    }

    public class WovenAttribute : Attribute
    {
    }
}
