using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ProjectPang
{
	public class SOBase : ScriptableObject
	{
		public virtual void UpdateData(List<string> dataList)
		{
			InitAllData();
		}

		/// <summary>
		/// 자식 클래스의 필드 값 모두 초기화 함수
		/// </summary>
		private void InitAllData()
		{
			var type = GetType();
			var fields = type.GetFields(
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.Instance |
				BindingFlags.DeclaredOnly);

			foreach (var field in fields)
			{
				var defaultValue = field.FieldType.IsValueType
					? Activator.CreateInstance(field.FieldType) // int, float, bool 등
					: null; // string, List, class 등 참조형은 null로

				field.SetValue(this, defaultValue);
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Script 새로 생성하는 함수
		/// </summary>
		/// <param name="className"></param>
		/// <param name="dataList"></param>
		/// <returns></returns>
		public static string GetNewScript(string className, List<string> dataList)
		{
			var sb = new StringBuilder();

			// using 및 네임스페이스
			sb.AppendLine("using UnityEngine;");
			sb.AppendLine("using System;");
			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine("using UnityEditor;");
			sb.AppendLine();
			sb.AppendLine("namespace ProjectPang");
			sb.AppendLine("{");
			sb.AppendLine(
				$"	[CreateAssetMenu(fileName = \"{className}\", menuName = \"Scriptable Objects/{className}\")]");
			sb.AppendLine($"	public class {className} : SOBase");
			sb.AppendLine("	{");

			// ✅ 필드 목록 저장용
			var fields = new List<(string type, string name)>();

			foreach (var item in dataList)
			{
				// #으로 시작하면 무시
				if (item.StartsWith("#"))
				{
					continue;
				}

				// 괄호 없는 항목 무시
				var openParen = item.IndexOf('(');
				var closeParen = item.IndexOf(')');
				if (openParen < 0 || closeParen < 0 || closeParen < openParen)
				{
					continue;
				}

				var variableName = item.Substring(0, openParen).Trim();
				var typeName = item.Substring(openParen + 1, closeParen - openParen - 1).Trim();

				fields.Add((typeName, variableName));

				sb.AppendLine($"		public {typeName} {variableName};");
			}

			// ✅ UpdateData 메서드 생성
			sb.AppendLine();
			sb.AppendLine("	#if UNITY_EDITOR");
			sb.AppendLine("		public override void UpdateData(List<string> dataList)");
			sb.AppendLine("		{");
			sb.AppendLine("			base.UpdateData(dataList);");
			sb.AppendLine();

			for (var i = 0; i < fields.Count; i++)
			{
				var (type, name) = fields[i];

				// List 타입 처리
				if (type.StartsWith("List"))
				{
					var genericType =
						type.Substring(type.IndexOf('<') + 1, type.IndexOf('>') - type.IndexOf('<') - 1);

					sb.AppendLine($"			if (dataList.Count > {i} && !string.IsNullOrEmpty(dataList[{i}]))");
					sb.AppendLine("			{");
					sb.AppendLine($"				{name} = new List<{genericType}>();");
					sb.AppendLine($"				foreach (var val in dataList[{i}].Split(','))");
					sb.AppendLine("				{");
					sb.AppendLine("					try");
					sb.AppendLine("					{");
					sb.AppendLine($"						var targetType = typeof({genericType});");
					sb.AppendLine($"						object converted;");
					sb.AppendLine("						if (targetType.IsEnum)");
					sb.AppendLine("							converted = Enum.Parse(targetType, val.Trim());");
					sb.AppendLine("						else");
					sb.AppendLine("							converted = Convert.ChangeType(val.Trim(), targetType);");
					sb.AppendLine($"						{name}.Add(({genericType})converted);");
					sb.AppendLine("					}");
					sb.AppendLine("					catch { }");
					sb.AppendLine("				}");
					sb.AppendLine("			}");
				}
				else // 일반 타입 처리
				{
					sb.AppendLine($"			if (dataList.Count > {i} && !string.IsNullOrEmpty(dataList[{i}]))");
					sb.AppendLine("			{");
					sb.AppendLine("				try");
					sb.AppendLine("				{");
					sb.AppendLine($"					var targetType = typeof({type});");
					sb.AppendLine($"					if (targetType.IsEnum)");
					sb.AppendLine($"						{name} = ({type})Enum.Parse(targetType, dataList[{i}]);");
					sb.AppendLine("					else");
					sb.AppendLine($"						{name} = ({type})Convert.ChangeType(dataList[{i}], targetType);");
					sb.AppendLine("				}");
					sb.AppendLine("				catch { }");
					sb.AppendLine("			}");
				}
			}

			sb.AppendLine();
			sb.AppendLine("			EditorUtility.SetDirty(this);");
			sb.AppendLine("			AssetDatabase.SaveAssets();");
			sb.AppendLine("			AssetDatabase.Refresh();");
			sb.AppendLine("		}");
			sb.AppendLine("	#endif");
			sb.AppendLine("	}");
			sb.AppendLine("}");

			return sb.ToString();
		}
#endif
	}
}
