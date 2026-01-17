using System;
using System.Collections.Generic;
using System.Linq;
using OFDViewer.Models.BaseType;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 集合操作辅助类
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// 创建一个同步字符串列表，当列表发生变化时会同步更新对应的ST_Loc集合
        /// </summary>
        /// <param name="updateCallback">同步更新回调函数</param>
        /// <param name="initialData">初始数据</param>
        /// <returns>同步字符串列表</returns>
        public static SynchronizedStringList CreateSynchronizedStringList(Action<List<ST_Loc>> updateCallback, List<string> initialData = null)
        {
            return new SynchronizedStringList(updateCallback, initialData);
        }
    }

    /// <summary>
    /// 同步字符串列表类，继承自List<string>
    /// 当用户操作列表时，会同步更新对应的ST_Loc集合
    /// </summary>
    public class SynchronizedStringList : List<string>
    {
        private readonly Action<List<ST_Loc>> _updateCallback;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="updateCallback">同步更新回调函数</param>
        /// <param name="initialData">初始数据</param>
        public SynchronizedStringList(Action<List<ST_Loc>> updateCallback, List<string> initialData) : base(initialData ?? new List<string>())
        {
            _updateCallback = updateCallback;
        }

        #region 重写修改方法以实现同步更新
        public new void Add(string item)
        {
            base.Add(item);
            SyncToST_LocList();
        }

        public new void Clear()
        {
            base.Clear();
            SyncToST_LocList();
        }

        public new void Insert(int index, string item)
        {
            base.Insert(index, item);
            SyncToST_LocList();
        }

        public new bool Remove(string item)
        {
            bool result = base.Remove(item);
            if (result)
            {
                SyncToST_LocList();
            }
            return result;
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            SyncToST_LocList();
        }

        public new void AddRange(IEnumerable<string> collection)
        {
            base.AddRange(collection);
            SyncToST_LocList();
        }

        public new void InsertRange(int index, IEnumerable<string> collection)
        {
            base.InsertRange(index, collection);
            SyncToST_LocList();
        }

        public new int RemoveAll(Predicate<string> match)
        {
            int count = base.RemoveAll(match);
            if (count > 0)
            {
                SyncToST_LocList();
            }
            return count;
        }

        public new void Reverse()
        {
            base.Reverse();
            SyncToST_LocList();
        }

        public new void Reverse(int index, int count)
        {
            base.Reverse(index, count);
            SyncToST_LocList();
        }

        public new void Sort()
        {
            base.Sort();
            SyncToST_LocList();
        }

        public new void Sort(IComparer<string> comparer)
        {
            base.Sort(comparer);
            SyncToST_LocList();
        }

        public new void Sort(Comparison<string> comparison)
        {
            base.Sort(comparison);
            SyncToST_LocList();
        }

        public new void Sort(int index, int count, IComparer<string> comparer)
        {
            base.Sort(index, count, comparer);
            SyncToST_LocList();
        }

        public new string this[int index]
        {
            get => base[index];
            set
            {
                base[index] = value;
                SyncToST_LocList();
            }
        }
        #endregion

        /// <summary>
        /// 将字符串列表同步到ST_Loc集合
        /// </summary>
        private void SyncToST_LocList()
        {
            var stLocList = this.Select(item => new ST_Loc(item)).ToList<ST_Loc>();
            _updateCallback?.Invoke(stLocList);
        }
    }
}