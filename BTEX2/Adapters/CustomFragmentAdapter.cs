using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.App;
using Android.Views;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace BitCWallet
{
    public class CustomFragmentAdapter : FragmentStatePagerAdapter
    {
        public List<Fragment> Fragments { get; private set; }
        private readonly Context _Context;
        public FragmentManager _Fm;
        public CustomFragmentAdapter(List<Fragment> fragments, FragmentManager fm, Context context) : base(fm)
        {
            Fragments = fragments;
            _Context = context;
            _Fm = fm;
        }
        public override int Count
        {
            get { return Fragments.Count; }
        }
        public override Fragment GetItem(int position)
        {
            return Fragments[position];
        }
        public override int GetItemPosition(Java.Lang.Object objectValue)
        {
            Fragment fragment = (Fragment)objectValue;
            int index = Fragments.IndexOf(fragment);
            if (index >= 0) return index;
            else return PositionNone;
        }
        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            base.DestroyItem(container, position, @object);
            if (position >= Count)
            {
                FragmentManager manager = ((Fragment)@object).FragmentManager;
                FragmentTransaction trans = manager.BeginTransaction();
                trans.Remove((Fragment)@object);
                trans.Commit();
            }
        }

        public int GetItemPosition(Type type)
        {
            return Fragments.FindIndex(x => x.GetType() == type);
        }
      
        public Fragment GetFragment(int position)
        {
            return GetItem(position);
        }
        public Fragment GetFragment(Type type)
        {
            var index = GetItemPosition(type);
            if (index >= 0) return GetItem(index);
            else return null;
        }
    }
}