﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDemo
{
    /// <summary>
    /// 无需处理 字节数组 和 编码
    /// 一行一行的读写 无需控制流的位置 他会自己寻找换行符
    /// 默认 UTF-8编码 缓冲数组bufferSize1024 可以在构造函数中 指定
    /// detectEncodingFromByteOrderMarks 表示是否都序言确定编码
    /// 
    /// 在读取文件时，流会自动确定下一个回车符的位置，并在该处停止读取。
    /// 在写入文件时，流会自动把回车符和换行符追加到文本的末尾。
    /// 
    /// 他是一行一行的读取，所有 position 没有用，等于说是一次性读取了 bufferSize 个数据，然后一行一行的输出，剩下的还在缓存区呢
    /// 等缓存区全部读完了，才开始下一次读取
    /// 
    /// </summary>
    class StreamReaderDemo
    {
        public void WriteFileUsingWriter(string line)
        {
            FileStream outputStream = File.OpenWrite(tempTextFileName);
            using (var writer = new StreamWriter(outputStream))
            {
                //获取UTF-8代表的序言转换的字节数组
                byte[] preable = Encoding.UTF8.GetPreamble();
                //先写入编码格式
                outputStream.Write(preable, 0, preable.Length);
                //在写入字符串数组
                writer.Write(line);
            }
        }

        public void ReadFileUsingReader()
        {
            FileStream stream = new FileStream(tempTextFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            //该构造方法接收一个Stream对象
            using (var reader = new StreamReader(stream))
            {
                //当前流位置是否在流末尾
                while (!reader.EndOfStream)
                {
                    long pos = reader.BaseStream.Position;
                    string line = reader.ReadLine();
                    Console.WriteLine(line + $"\n(position at {pos})");
                }
            }
        }

        string tempTextFileName = Program.tempTextFileName;
    }
}
