using UnityEngine;
using System;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq;

public class QuasarApiExporter : MonoBehaviour 
{
    void Start()
    {
        // ПОЛНЫЙ список классов для создания аналога Gaea
        Type[] typesToExport = {
            typeof(ComputeShader),    // ЯДРО: Запуск вычислений на GPU (Dispatch, SetTexture)
            typeof(ComputeBuffer),    // ДАННЫЕ: Передача массивов чисел в шейдер (эрозия)
            typeof(RenderTexture),    // ХОЛСТ: Карта высот в видеопамяти
            typeof(TerrainData),      // ЗЕМЛЯ: Система высот ландшафта
            typeof(Terrain),          // ОБЪЕКТ: Настройки отображения земли
            typeof(Texture2D),        // КАРТИНКА: Импорт/Экспорт (EncodeToPNG)
            typeof(Mathf),            // МАТЕМАТИКА: Шумы, интерполяция, синусы
            typeof(Mesh),             // ГЕОМЕТРИЯ: Если будешь генерировать сетку сам
            typeof(Graphics)          // РЕНДЕР: Команды отрисовки (Blit, DrawMesh)
        };

        StringBuilder sb = new StringBuilder();

        foreach (Type type in typesToExport)
        {
            if (type == null) continue;

            sb.AppendLine($"=== CLASS: {type.FullName} ===");
            sb.AppendLine($"Module: {type.Assembly.GetName().Name}"); // Чтобы знать, какую DLL копировать
            
            // 1. Свойства (Properties)
            sb.AppendLine("-- PROPERTIES --");
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                sb.AppendLine($"prop {prop.PropertyType.Name} {prop.Name}");
            }

            // 2. Методы (API)
            sb.AppendLine("\n-- METHODS --");
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (method.IsSpecialName) continue; 

                var parameters = method.GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}");

                // Твой формат: function Name(args) -> returns Type
                sb.AppendLine($"function {method.Name}({string.Join(", ", parameters)}) -> returns {method.ReturnType.Name}");
            }
            sb.AppendLine("\n" + new string('=', 50) + "\n");
        }

        // Создаем путь и сохраняем
        string dir = Path.Combine(Application.dataPath, "Plugins/_api/");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        string path = Path.Combine(dir, "reflection.txt");
        File.WriteAllText(path, sb.ToString());
        
        Debug.Log($"<color=cyan><b>[QUASAR] API для Gaea успешно выгружено:</b></color> {path}");
    }
}
