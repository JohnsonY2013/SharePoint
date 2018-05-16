using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace hp.utilities
{
	public class XMLSettings : XmlDocument
	{
		public string SettingsName = "";

		public XMLSettings(string SettingsName)
		{
			this.SettingsName = SettingsName.Replace(' ', '_');
			this.AppendChild(this.CreateElement(this.SettingsName));
		}

		public string ID
		{
			get
			{
				if (this.DocumentElement.Attributes["ID"] != null)
					return this.DocumentElement.Attributes["ID"].InnerText;
				return "";
			}
		}

		public string ApplicationName
		{
			get
			{
				if (this.DocumentElement.Attributes["ApplicationName"] != null)
					return this.DocumentElement.Attributes["ApplicationName"].InnerText;
				return "";
			}
		}

		public string ApplicationDescription
		{
			get
			{
				if (this.DocumentElement.Attributes["ApplicationDescription"] != null)
					return this.DocumentElement.Attributes["ApplicationDescription"].InnerText;
				return "";
			}
		}

		public string Version
		{
			get
			{
				if (this.DocumentElement.Attributes["Version"] != null)
					return this.DocumentElement.Attributes["Version"].InnerText;
				return "";
			}
		}

		public void Set(string Path, string Attribute, object Value)
		{
			string[] PathData = Path.Split('\t');
			if (PathData.Length > 0)
			{
				XmlNode ThisNode = this.DocumentElement;
				for (int i = 0; i < PathData.Length; i++)
				{
					string ThisNodeName = XmlConvert.EncodeName(PathData[i]);
					XmlNode NextNode = ThisNode[ThisNodeName];
					if (NextNode == null)
					{
						NextNode = this.CreateElement(ThisNodeName);
						ThisNode.AppendChild(NextNode);
					}
					ThisNode = NextNode;
				}
				if (Value == null)
				{
					if (Attribute == "Value")
						((XmlElement)ThisNode).SetAttribute("Type", "NULL");
					((XmlElement)ThisNode).SetAttribute(Attribute, "NULL");
				}
				else
				{
					if (Attribute == "Value")
						((XmlElement)ThisNode).SetAttribute("Type", Value.GetType().Name);
					if (Value.GetType().Equals(typeof(DateTime)))
						((XmlElement)ThisNode).SetAttribute(Attribute, ((DateTime)Value).ToString(DateTimeFormatInfo.InvariantInfo));
					else
						((XmlElement)ThisNode).SetAttribute(Attribute, Value.ToString());
				}
			}
		}

		public void Set(string Path, object Value)
		{
			Set(Path, "Value", Value);
		}

		public XmlNode Find(string Path)
		{
			string[] PathData = Path.Split('\t');
			if (PathData.Length > 0)
			{
				XmlNode ThisNode = this.DocumentElement;
				for (int i = 0; i < PathData.Length && ThisNode != null; i++)
				{
					string ThisNodeName = XmlConvert.EncodeName(PathData[i]);
					ThisNode = ThisNode[ThisNodeName];
				}
				return ThisNode;
			}
			return null;
		}

		public bool Exists(string Path)
		{
			return Find(Path) != null;
		}

		public void Delete(string Path)
		{
			XmlNode ThisNode = Find(Path);
			if (ThisNode != null)
				ThisNode.ParentNode.RemoveChild(ThisNode);
		}

		public string[] GetNames(string Path)
		{
			List<string> Names = new List<string>();
			XmlNode ThisNode = Find(Path);
			if (ThisNode != null)
			{
				foreach (XmlNode ThisChildNode in ThisNode.ChildNodes)
					Names.Add(XmlConvert.DecodeName(ThisChildNode.Name));
			}
			return Names.ToArray();
		}

		public string GetAttribute(string Path, string Attribute)
		{
			try
			{
				XmlNode ThisNode = Find(Path);
				if (ThisNode != null)
				{
					if (ThisNode.Attributes[Attribute] != null)
						return ThisNode.Attributes[Attribute].InnerText;
				}
			}
			catch { }
			return null;
		}

		public static Type ParseType(string TypeName)
		{
			switch (TypeName)
			{
				case "Boolean":
					return typeof(Boolean);

				case "Byte":
					return typeof(Byte);

				case "Char":
					return typeof(Char);

				case "Int16":
					return typeof(Int16);

				case "UInt16":
					return typeof(UInt16);

				case "Int32":
					return typeof(Int32);

				case "UInt32":
					return typeof(UInt32);

				case "Single":
					return typeof(Single);

				case "Double":
					return typeof(Double);

				case "Decimal":
					return typeof(Decimal);

				case "Date":
				case "DateTime":
					return typeof(DateTime);

				case "Time":
				case "TimeSpan":
					return typeof(TimeSpan);

				case "NULL":
					return null;

				default:
					return typeof(String);
			}
		}

		public Type GetType(string Path)
		{
			try
			{
				XmlNode ThisNode = Find(Path);
				if (ThisNode != null)
				{
				    if (ThisNode.Attributes["Type"] != null)
						return ParseType(ThisNode.Attributes["Type"].InnerText);
				    return typeof(String);
				}
			}
			catch { }
			return null;
		}

		public object GetValue(string Path)
		{
			try
			{
				XmlNode ThisNode = Find(Path);
				if (ThisNode != null)
				{
					string Value = ThisNode.Attributes["Value"].InnerText;
					if (ThisNode.Attributes["Type"] != null)
					{
						switch (ThisNode.Attributes["Type"].InnerText)
						{
							case "Boolean":
								return Boolean.Parse(Value);

							case "Byte":
								return Byte.Parse(Value);

							case "Char":
								return (Char)int.Parse(Value);

							case "Int16":
								return Int16.Parse(Value);

							case "UInt16":
								return UInt16.Parse(Value);

							case "Int32":
								return Int32.Parse(Value);

							case "UInt32":
								return UInt32.Parse(Value);

							case "Single":
								return Single.Parse(Value);

							case "Double":
								return Double.Parse(Value);

							case "Decimal":
								return Decimal.Parse(Value);

							case "DateTime":
								return DateTime.Parse(Value, DateTimeFormatInfo.InvariantInfo);

							case "NULL":
								return null;

							default:
								return Value;
						}
					}
				    return Value;
				}
			}
			catch { }
			return null;
		}

		public string GetString(string Path, string DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(string)))
				return (string)Value;
			return DefaultValue;
		}

		public int GetInt(string Path, int DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(int)))
				return (int)Value;
			return DefaultValue;
		}

		public UInt32 GetUInt32(string Path, UInt32 DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(UInt32)))
				return (UInt32)Value;
			return DefaultValue;
		}

		public Int16 GetInt16(string Path, Int16 DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(Int16)))
				return (Int16)Value;
			return DefaultValue;
		}

		public UInt16 GetUInt16(string Path, UInt16 DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(UInt16)))
				return (UInt16)Value;
			return DefaultValue;
		}

		public float GetFloat(string Path, float DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(float)))
				return (float)Value;
			return DefaultValue;
		}

		public double GetDouble(string Path, double DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(double)))
				return (double)Value;
			return DefaultValue;
		}

		public decimal GetDecimal(string Path, decimal DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(decimal)))
				return (decimal)Value;
			return DefaultValue;
		}

		public bool GetBool(string Path, bool DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(bool)))
				return (bool)Value;
			return DefaultValue;
		}

		public DateTime GetDateTime(string Path, DateTime DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(DateTime)))
				return (DateTime)Value;
			return DefaultValue;
		}

		public TimeSpan GetTimeSpan(string Path, TimeSpan DefaultValue)
		{
			object Value = GetValue(Path);
			if (Value != null && Value.GetType().Equals(typeof(TimeSpan)))
				return (TimeSpan)Value;
			return DefaultValue;
		}

	    public string GetInnerXml(string Path, string DefalutValue)
	    {
	        try
	        {
	            XmlNode ThisNode = Find(Path);
	            if (ThisNode != null)
	            {
	                return ThisNode.InnerXml;
	            }
	        }
	        catch
	        {
	        }
	        return DefalutValue;
	    }

	    public override void Load(string filename)
	    {
	        try
	        {
	            // Check if locale-specific file exists
	            if (
	                File.Exists(Path.ChangeExtension(filename,
	                    CultureInfo.CurrentCulture.Name + Path.GetExtension(filename))))
	                base.Load(Path.ChangeExtension(filename, CultureInfo.CurrentCulture.Name + Path.GetExtension(filename)));
	            else if (
	                File.Exists(Path.ChangeExtension(filename,
	                    CultureInfo.CurrentCulture.Parent.Name + Path.GetExtension(filename))))
	                base.Load(Path.ChangeExtension(filename,
	                    CultureInfo.CurrentCulture.Parent.Name + Path.GetExtension(filename)));
	            else
	                base.Load(filename);
	        }
	        catch
	        {
	            AppendChild(CreateElement(SettingsName));
	        }
	    }
	}
}
