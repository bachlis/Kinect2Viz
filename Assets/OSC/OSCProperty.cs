using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class OSCProperty : Attribute
{
    
    public readonly string address;

    public OSCProperty(string address)
    {
        this.address = address;
        
    }
}