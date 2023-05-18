using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TuoLuoUtils
{
    public static class SelectionTuoLuoUtils
    {
        const string _statusPrompt = "请选择图元";

        /// <summary>
        /// 通过图元类型进行选择,例：Wall,Pipe,Floor,FamilyInstance...
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T PickElementByElementType<T>(this Selection selection, Document doc, string statusPrompt = _statusPrompt, Guid? schemaId = null) where T : Element
        {
            ISelectionFilter selFilter = new ElementTypeSelectionFilter(typeof(T), schemaId);
            T elem = null;

            try
            {
                elem = (T)doc.GetElement(selection.PickObject(ObjectType.Element, selFilter, statusPrompt));
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                //ESC取消异常不抛出异常信息
            }
            return elem;
        }

        /// <summary>
        /// 通过图元类型进行选择,例：Wall,Pipe,Floor,FamilyInstance...
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T PickElementByElementType<T>(this UIDocument uiDoc, string statusPrompt = _statusPrompt, Guid? schemaId = null) where T : Element
        {
            Document doc = uiDoc.Document;
            Selection selection = uiDoc.Selection;
            ISelectionFilter selFilter = new ElementTypeSelectionFilter(typeof(T), schemaId);
            T elem = null;

            try
            {
                elem = (T)doc.GetElement(selection.PickObject(ObjectType.Element, selFilter, statusPrompt));
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                //ESC取消异常不抛出异常信息
            }
            return elem;
        }

        /// <summary>
        /// 通过矩形框和图元类型进行选择,例：Wall,Pipe,Floor,FamilyInstance...
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> PickElementsByElementType<T>(this Selection selection, string statusPrompt = _statusPrompt, Guid? schemaId = null)
        {
            ISelectionFilter selFilter = new ElementTypeSelectionFilter(typeof(T), schemaId);
            List<T> eList = null;
            try
            {
                eList = selection.PickElementsByRectangle(selFilter, statusPrompt).OfType<T>().ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                //ESC取消异常不抛出异常信息
            }

            return eList;
        }

        /// <summary>
        /// 通过矩形框和图元类型进行选择,例：Wall,Pipe,Floor,FamilyInstance...
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> PickElementsByElementType<T>(this UIDocument uiDoc, string statusPrompt = _statusPrompt, Guid? schemaId = null)
        {
            Selection selection = uiDoc.Selection;
            ISelectionFilter selFilter = new ElementTypeSelectionFilter(typeof(T), schemaId);
            List<T> eList = null;
            try
            {
                eList = selection.PickElementsByRectangle(selFilter, statusPrompt).OfType<T>().ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                //ESC取消异常不抛出异常信息
            }
            return eList;
        }

        public class ElementTypeSelectionFilter : ISelectionFilter
        {
            Type _Type = null;
            Guid? _SchemaId = null;

            public ElementTypeSelectionFilter(Type type, Guid? schemaId)
            {
                _Type = type;
                _SchemaId = schemaId;
            }

            public bool AllowElement(Element elem)
            {
                Type elemType = elem.GetType();

                if (_Type.IsAssignableFrom(elemType))
                {
                    if (_SchemaId != null)
                    {
                        IList<Guid> shcemaIds = elem.GetEntitySchemaGuids();

                        if (shcemaIds.Contains(_SchemaId ?? Guid.Empty)) return true;

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
    }
}