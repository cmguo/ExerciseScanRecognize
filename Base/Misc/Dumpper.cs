using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Base.Misc
{

    public class Dumpper
    {

        int mFd = 0;
        StreamWriter mWriter;
        private string mCommand;
        private bool mExactly = false;
        private bool mOnlyOne = false;
        private int mDetail = 0;
        string mPrefix = "";
        private string mSearch = null;
        string mPath = "";
        private Matcher mMatcher;

        public Dumpper(int fd, StreamWriter pw, bool exactly)
        {
            mFd = fd;
            mWriter = pw;
            mDetail = 2;
            List<string[]> pattens = new List<string[]>();
            int pathlevel = -1;
            mMatcher = new Matcher(this, pattens, pathlevel);
        }

        public Dumpper(StreamWriter pw, int detail)
        {
            mWriter = pw;
            mDetail = detail;
            mMatcher = new Matcher(this, new List<string[]>(), -1);
        }

        public void setDetail(int d)
        {
            mDetail = d;
        }

        public void reset()
        {
            mLastBeginPath = null;
            mLastEndPath = null;
        }

        public bool exactly()
        {
            return mExactly;
        }

        public bool onlyone()
        {
            return mOnlyOne;
        }

        public StreamWriter writer()
        {
            return mWriter;
        }

        public string prefix()
        {
            return mPrefix;
        }

        public int detail()
        {
            return mDetail;
        }

        public string path()
        {
            return mPath;
        }

        protected void setOption(int opt, string arg)
        {
        }

        public void dump(string title, object value)
        {
            title = formatTitle(title);
            string path = mPath;
            mPath = mPath + "/" + title;
            Matcher matcher = mMatcher;
            if (!matcher.mInPath)
            {
                Matcher matcher2 = new Matcher(mMatcher, title);
                if (!matcher2.mInPath)
                {
                    if (matcher2.mMatches.Count == 0)
                    {
                        mPath = path;
                        return;
                    }
                    // continue search
                    mMatcher = matcher2;
                    dump(value);
                    mMatcher = matcher;
                    mPath = path;
                    return;
                }
                mMatcher = matcher2;
                dumpRoot(mPath);
            }
            else if (mSearch != null)
            {
                if (mExactly ? title.EndsWith(mSearch)
                        : title.ToLower().Contains(mSearch))
                {
                    mWriter.WriteLine(mPath);
                    mPath = path;
                    return;
                }
            }
            if (mSearch == null)
                dumpTitle(title, value, matcher.mInPath);
            if (mDetail >= 0)
            {
                try
                {
                    dump(value);
                }
                catch
                {
                }
            }
            else
            {
                dumpValue(value);
            }
            mMatcher = matcher;
            if (mOnlyOne)
                mMatcher.mMatches.Clear();
            mPath = path;
        }

        public void dump(object value)
        {
            if (value is IEnumerable enumerable)
            {
                int i = 0;
                childBegin('A');
                foreach (object o in enumerable)
                {
                    dump("#" + i, o);
                    ++i;
                }
                childEnd();
            }
            else if (value is IDictionary dictionary)
            {
                childBegin('M');
                foreach (var e in dictionary.Keys)
                {
                    dump(stringOf(e), dictionary[e]);
                }
                childEnd();
            }
            else
            {
                dumpValue(value);
            }
        }

        void dumpRuntime(Type type, object obj)
        {
            try
            {
                foreach (PropertyInfo f in type.GetProperties())
                {
                    if (f == null)
                        continue;
                    dump(f.Name, f.GetValue(obj));
                }
            }
            catch
            {
            }
        }

        protected void setCommand(string command)
        {
            mCommand = command;
        }

        // for each match path
        protected void dumpRoot(string path)
        {
        }

        // for each title value pair, include match root
        protected void dumpTitle(string title, object value, bool inPath)
        {
            if (inPath)
            {
                dumpTitle(title);
            }
        }

        // for each title, not include match root
        protected void dumpTitle(string title)
        {
        }

        // for each child begin, except child with no title
        protected void dumpBegin(char type)
        {
            --mDetail;
            mPrefix += "  ";
        }

        // for each child end, except child with no title
        protected void dumpEnd()
        {
            mPrefix = mPrefix.Substring(0, mPrefix.Length - 2);
            ++mDetail;
        }

        // for each simple value
        protected void dumpValue(object value)
        {
        }

        protected string formatTitle(string title)
        {
            return title;
        }

        protected string stringOf(object value)
        {
            if (value is ICollection collection)
            {
                return value.GetType().Name +
                    "[" + (collection.Count) + "]";
            }
            else if (value is KeyValuePair<int, int> pair)
            {
                return stringOf(pair.Key + " --> " + stringOf(pair.Value));
            }
            else if (value == null)
            {
                return "<null>";
            }
            else
            {
                return value.ToString();
            }
        }

        protected char typeOf(object value)
        {
            if (value is IEnumerable)
                return 'A';
            if (value is IDictionary)
                return 'M';
            return (char)(0);
        }


        private string mLastBeginPath;
        private string mLastEndPath;

        private void childBegin(char type)
        {
            if (!mMatcher.mInPath)
                return;
            if (mLastBeginPath == mPath)
            {
                return;
            }
            mLastBeginPath = mPath;
            dumpBegin(type);
        }

        private void childEnd()
        {
            if (!mMatcher.mInPath)
                return;
            if (mLastEndPath == mPath)
            {
                return;
            }
            mLastEndPath = mPath;
            dumpEnd();
        }

        internal class Matcher
        {
            Dumpper mOwner;
            int mLevel;
            internal List<string[]> mMatches;
            internal bool mInPath;
            internal Matcher(Dumpper owner, List<string[]> paths, int level)
            {
                mOwner = owner;
                mLevel = level;
                mMatches = paths;
                mInPath = mMatches.Count == 0;
            }

            internal Matcher(Matcher parent, string name)
            {
                mOwner = parent.mOwner;
                mLevel = parent.mLevel + 1;
                mInPath = parent.mInPath;
                mMatches = parent.mMatches;
                if (!mInPath)
                {
                    if (!mOwner.mExactly)
                        name = name.ToLower();
                    //Log.d(TAG, "Path level=" + mLevel + ", name=" + name);
                    List<string[]> matches = new List<string[]>();
                    foreach (string[] path in mMatches)
                    {
                        //Log.d(TAG, "Path try match " + Arrays.tostring(path));
                        if (mOwner.mExactly ? name.Equals(path[mLevel])
                                : name.Contains(path[mLevel]))
                        {
                            //Log.d(TAG, "Path match " + Arrays.tostring(path));
                            matches.Add(path);
                            if (path.Length == mLevel + 1)
                            {
                                mInPath = true;
                            }
                        }
                    }
                    mMatches = matches;
                }
            }

            string[] buildArgs(Dumpper d)
            {
                int argc = 4; // command detail, prefix, path
                if (d.mSearch != null)
                    ++argc;
                if (!mInPath)
                {
                    ++argc;
                    argc += mMatches.Count;
                }
                if (mOwner.mExactly)
                    ++argc;
                if (mOwner.mOnlyOne)
                    ++argc;
                string[] args = new string[argc];
                argc = 0;
                args[argc++] = mOwner.mCommand;
                if (mOwner.mExactly)
                    args[argc++] = "--exactly";
                if (mOwner.mOnlyOne)
                    args[argc++] = "--onlyone";
                args[argc++] = "--detail=" + d.mDetail;
                args[argc++] = "--prefix=" + d.mPrefix;
                args[argc++] = "--path=" + d.mPath;
                if (d.mSearch != null)
                    args[argc++] = "--search=" + d.mPath;
                if (!mInPath)
                {
                    args[argc++] = "--pathlevel=" + mLevel;
                    foreach (string[] patten in mMatches)
                    {
                        args[argc++] = String.Join("/", patten);
                    }
                }
                return args;
            }
        }

    }

}
