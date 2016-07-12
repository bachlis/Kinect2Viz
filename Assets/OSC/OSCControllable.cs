using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;

public class OSCControllable : MonoBehaviour {


    public string oscName;
    public List<KeyValuePair<string,FieldInfo>> oscProperties;

    void init()
    {
        oscProperties = new List<KeyValuePair<string, FieldInfo>>();

        Type t = GetType();
        FieldInfo[] objectFields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);

        for (int i = 0; i < objectFields.Length; i++)
        {
            FieldInfo info = objectFields[i];
            OSCProperty attribute = Attribute.GetCustomAttribute(info, typeof(OSCProperty)) as OSCProperty;
            
            if (attribute != null)
            {
                oscProperties.Add(new KeyValuePair<string, FieldInfo>(attribute.address,info));
            }
        }
    }



    public void setProp(string property, List<object> values)
    {
        if (oscProperties == null) init();

        FieldInfo info = getPropInfoForAddress(property);

        if (info == null)
        {
            Debug.Log("not found : " + property);
            return;
        }

        string typeString = info.FieldType.ToString();

        //Debug.Log("OSC IN TYPE : " + typeString +" " + values[0].ToString());

        // if we detect any attribute print out the data.

                if (typeString == "System.Single")
        {
            if(values.Count >= 1) info.SetValue(this, getFloat(values[0]));
        }else if(typeString == "System.Boolean")
        {
            if (values.Count >= 1) info.SetValue(this, getBool(values[0]));
        }
        else if(typeString == "System.Int32")
        {
            if (values.Count >= 1) info.SetValue(this, getInt(values[0]));
        } else if (typeString == "UnityEngine.Vector2")
        {
            if (values.Count >= 2) info.SetValue(this, new Vector2(getFloat(values[0]), getFloat(values[1])));
        }
        else if (typeString == "UnityEngine.Vector3")
        {
            if (values.Count >= 3) info.SetValue(this, new Vector3(getFloat(values[0]), getFloat(values[1]), getFloat(values[2])));
        }
        else if (typeString == "UnityEngine.Color")
        {
            if (values.Count >= 4) info.SetValue(this, new Color(getFloat(values[0]), getFloat(values[1]), getFloat(values[2]), getFloat(values[3])));
            else if(values.Count >= 3) info.SetValue(this, new Color(getFloat(values[0]), getFloat(values[1]), getFloat(values[2]),1));
        }
        else if (typeString == "System.String")
        {
           // Debug.Log("String received : " + values.ToString());
            info.SetValue(this, values[0].ToString());
        }
    }


    public float getFloat(object value)
    {
        Type t = value.GetType();
        if (t == typeof(float)) return (float)value;
        if (t == typeof(int)) return (float)((int)value);
        if (t == typeof(string)) return float.Parse((string)value);
        if (t == typeof(bool)) return (bool)value ? 1 : 0;
        
        return float.NaN;
    }

    public int getInt(object value)
    {
        Type t = value.GetType();
        if (t == typeof(float)) return (int)((float)value);
        if (t == typeof(int)) return (int)value;
        if (t == typeof(string)) return int.Parse((string)value);
        if (t == typeof(bool)) return (bool)value ? 1 : 0;

        return 0;
    }

    public bool getBool(object value)
    {
        Type t = value.GetType();
        if (t == typeof(float)) return (float)value >= 1;
        if (t == typeof(int)) return (int)value >= 1;
        if (t == typeof(string)) return int.Parse((string)value) >= 1;
        if (t == typeof(bool)) return (bool)value;

        return false;
    }

    public FieldInfo getPropInfoForAddress(string address)
    {
        foreach(KeyValuePair<string, FieldInfo> p in oscProperties)
        {
            if(p.Key == address)
            {
                return p.Value; 
            }
        }

        return null;
    }

    public virtual void Start() { }
    public virtual void Update() { }

}
