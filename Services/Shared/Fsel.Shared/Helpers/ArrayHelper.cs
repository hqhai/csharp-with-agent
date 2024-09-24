// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class ArrayHelper
    {
        /// <summary>
        /// Loại bỏ số phần tử trong mảng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="numberOfElemment"></param>
        /// <returns></returns>
        public static IList<T> RemoveFirstTwoElements<T>(IList<T> list, int numberOfElemment)
        {
            if (list.Count >= numberOfElemment)
            {
                return list.Skip(numberOfElemment).ToList();
            }
            else
            {
                // Danh sách rỗng, trả về danh sách rỗng
                return new List<T>();
            }
        }
    }
}
