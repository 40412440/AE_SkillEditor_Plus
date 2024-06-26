﻿using System;
using System.Collections.Generic;
using AE_SkillEditor_Plus.Editor.UI.Controller;
using AE_SkillEditor_Plus.Event;
using AE_SkillEditor_Plus.UI;
using AE_SkillEditor_Plus.UI.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using EventType = AE_SkillEditor_Plus.Event.EventType;


namespace AE_SkillEditor_Plus
{
    public class ClipEditorWindow : EditorWindow
    {
        #region Static

        public static string Title = "AEClip编辑器";

        //打开窗口
        [MenuItem("Tools/AEClip编辑器")]
        public static void OpenWindiw()
        {
            var window = CreateWindow<ClipEditorWindow>(Title);
        }

        #endregion

        #region Test

        //TODO: 测试数据
        private List<TrackStyleData> trackStyleData;

        #endregion

        private int TrackHeight = 30; //轨道高度
        private float widthPreFrame = 1f;

        private float WidthPreFrame
        {
            get { return widthPreFrame; }
            set
            {
                if ((value) <= 0.005f) widthPreFrame =0.005f;
                else if (value >= 140f) widthPreFrame = 140f;
                else widthPreFrame = value;
            }
        } //每帧多宽

        private int HeadWidth = 250; //轨道头部宽度
        private int IntervalHeight = 10; //轨道间隔
        private int ControllerHeight = 30; //控件高度
        private int currentFrameID;

        private int CurrentFrameID
        {
            get { return currentFrameID; }
            set
            {
                if (value < 0) return;
                if (currentFrameID == value) return;
                currentFrameID = value;
                CurrentFrameIDChange();
            }
        } //当前帧

        private object tempObject; //临时的一个变量 保存CtrlC的数据

        private int mouseCurrentFrameID //鼠标位置转化为FrameID
            => (int)((UnityEngine.Event.current.mousePosition.x - HeadWidth) / WidthPreFrame);

        //初始化
        private void OnEnable()
        {
            EventCenter.AddEventListener(this, ProcessEvent);

            //TODO: 测试数据
            trackStyleData = new List<TrackStyleData>()
            {
                new TrackStyleData()
                {
                    Name = "测试Track1",
                    Color = Color.green,
                    Clips = new List<ClipStyleData>()
                    {
                        new ClipStyleData()
                            { Color = Color.red, StartID = 600, EndID = 800, Name = "测试Clip1" },
                        new ClipStyleData()
                            { Color = Color.red, StartID = 0, EndID = 100, Name = "测试Clip2" }
                    }
                },
                new TrackStyleData()
                {
                    Name = "测试Track2",
                    Color = Color.green,
                    Clips = new List<ClipStyleData>()
                    {
                        new ClipStyleData()
                            { Color = Color.red, StartID = 550, EndID = 660, Name = "测试Clip1" },
                        new ClipStyleData()
                            { Color = Color.red, StartID = 110, EndID = 450, Name = "测试Clip2" }
                    }
                },
                new TrackStyleData()
                {
                    Name = "测试Track3",
                    Color = Color.green,
                    Clips = new List<ClipStyleData>()
                    {
                        new ClipStyleData()
                            { Color = Color.red, StartID = 200, EndID = 400, Name = "测试Clip1" },
                        new ClipStyleData()
                            { Color = Color.red, StartID = 600, EndID = 700, Name = "测试Clip2" }
                    }
                }
            };
        }

        //销毁
        private void OnDestroy()
        {
            EventCenter.RemoveEventListener(this, ProcessEvent);
        }

        //更新UI
        private void OnGUI()
        {
            //遍历轨道
            int height = 0;
            height += ControllerHeight;
            for (var index = 0; index < trackStyleData.Count; index++)
            {
                var trackData = trackStyleData[index];
                var track = new TrackStyle();
                //为轨道划分Rect
                var rect = new Rect(0, height, position.width, TrackHeight);
                track.UpdateUI(this, rect, WidthPreFrame, HeadWidth, trackData, index);
                height += TrackHeight + IntervalHeight;
            }

            //划分控件 Timeline
            var controllerRect = new Rect(0, 2.5f, HeadWidth - 10f, ControllerHeight - 2.5f);
            var timelineRect = new Rect(HeadWidth, 2.5f, position.width - HeadWidth, ControllerHeight - 2.5f);
            //绘制控件和Timeline
            Controller.UpdateGUI(this, controllerRect);
            TimeLine.UpdateGUI(this, CurrentFrameID, WidthPreFrame, timelineRect);
        }

        //当前帧改变事件
        private void CurrentFrameIDChange()
        {
            Repaint();
        }

        //处理事件
        private void ProcessEvent(BaseEvent baseEvent)
        {
            switch (baseEvent.EventType)
            {
                //Clip事件
                case EventType.ClipMove:
                {
                    ClipMove((ClipMoveEvent)baseEvent);
                    break;
                }
                case EventType.ClipResize:
                {
                    ClipResize((ClipResizeEvent)baseEvent);
                    break;
                }
                case EventType.ClipKeyborad:
                {
                    ProcessKeyborad((KeyboradEvent)baseEvent);
                    break;
                }
                //控件事件
                case EventType.Controller:
                {
                    ProcessController((ControllerEvent)baseEvent);
                    break;
                }
                //Timeline
                case EventType.TimelineScale:
                {
                    ProcessTimelineScale((TimelineScaleEvent)baseEvent);
                    break;
                }
                case EventType.TimelineDrag:
                {
                    ProcessTimelineDrag((TimelineDragEvent)baseEvent);
                    break;
                }
            }
        }

        //在时间轴上拖动
        private void ProcessTimelineDrag(TimelineDragEvent dragEvent)
        {
            CurrentFrameID = mouseCurrentFrameID;
        }

        //时轴缩放
        private void ProcessTimelineScale(TimelineScaleEvent scaleEvent)
        {
            // Debug.Log(scaleEvent.EventType + "  " + UnityEngine.Event.current.delta.y);
            WidthPreFrame -= Mathf.Sign(UnityEngine.Event.current.delta.y) * WidthPreFrame * 0.2f;
            // Debug.Log(WidthPreFrame);
            //仅仅是+=1的话没有卡死
            Repaint();
        }

        //控件事件
        private void ProcessController(ControllerEvent controller)
        {
            switch (controller.ControllerType)
            {
                case ControllerType.ToMostBegin:
                    CurrentFrameID = 0;
                    break;
                case ControllerType.ToPre:
                    CurrentFrameID -= 1;
                    break;
                case ControllerType.Play: break;
                case ControllerType.ToNext:
                    CurrentFrameID += 1;
                    break;
                case ControllerType.ToMostEnd:
                    CurrentFrameID += 1;
                    break;
            }
        }

        //Clip大小改变
        private void ClipResize(ClipResizeEvent clipResize)
        {
            trackStyleData[clipResize.TrackIndex].Clips[clipResize.ClipIndex].EndID =
                (int)(mouseCurrentFrameID - (clipResize.OffsetMouseX / WidthPreFrame));
            Repaint();
        }

        //Clip移动事件
        private void ClipMove(ClipMoveEvent clipMove)
        {
            int originStart = trackStyleData[clipMove.TrackIndex].Clips[clipMove.ClipIndex].StartID;
            trackStyleData[clipMove.TrackIndex].Clips[clipMove.ClipIndex].StartID =
                (int)(mouseCurrentFrameID - (clipMove.OffsetMouseX / WidthPreFrame));
            trackStyleData[clipMove.TrackIndex].Clips[clipMove.ClipIndex].EndID =
                trackStyleData[clipMove.TrackIndex].Clips[clipMove.ClipIndex].StartID +
                trackStyleData[clipMove.TrackIndex].Clips[clipMove.ClipIndex].EndID - originStart;
            Repaint();
        }

        //处理按键事件
        public void ProcessKeyborad(KeyboradEvent keyborad)
        {
            //TODO：测试数据
            Debug.Log(keyborad.Shortcut + "--" + keyborad.TrackIndex + "--" + keyborad.ClipIndex);
            switch (keyborad.Shortcut)
            {
                case Shortcut.CtrlC:
                    tempObject = new ClipStyleData()
                    {
                        StartID = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].StartID,
                        EndID = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].EndID,
                        Name = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].Name,
                        Color = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].Color
                    };
                    break;
                case Shortcut.CtrlV:
                    AddClip(trackStyleData, keyborad.TrackIndex, tempObject);
                    var newTemp = new ClipStyleData()
                    {
                        StartID = (tempObject as ClipStyleData).StartID,
                        EndID = (tempObject as ClipStyleData).EndID,
                        Name = (tempObject as ClipStyleData).Name,
                        Color = (tempObject as ClipStyleData).Color
                    };
                    tempObject = newTemp;
                    break;
                case Shortcut.CtrlX:
                    tempObject = new ClipStyleData()
                    {
                        StartID = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].StartID,
                        EndID = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].EndID,
                        Name = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].Name,
                        Color = trackStyleData[keyborad.TrackIndex].Clips[keyborad.ClipIndex].Color
                    };
                    RemoveClip(trackStyleData, keyborad.TrackIndex, keyborad.ClipIndex);
                    break;
            }
        }

        //TODO: 测试数据
        private void AddClip(List<TrackStyleData> data, int trackIndex, object clip)
        {
            var clipData = (clip as ClipStyleData);
            //这里必须先得到length
            int length = clipData.EndID - clipData.StartID;
            clipData.StartID = mouseCurrentFrameID;
            clipData.EndID = mouseCurrentFrameID + length;
            data[trackIndex].Clips.Add(clipData);
            Repaint();
        }

        //TODO: 测试数据
        private void RemoveClip(List<TrackStyleData> data, int trackIndex, int clipIndex)
        {
            data[trackIndex].Clips.RemoveAt(clipIndex);
        }
    }
}