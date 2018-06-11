# Hololens_Heatmap_work

1. Connect Pupil labs hardware over the USB to the Remote PC
2. Start Pupil labs service on Remote PC
3. Start Unity Calibration Project for Pupil labs Calibration - for steps 3 and 4 download and follow from https://github.com/pupil-labs/hmd-eyes
4. Using the Hololens Remote connect application stream the calibration
program to the Hololens and perform the calibration
5. Start python script “gazestreamer.py”
6. Start python script “Heatmap Streamer.py”
7. Start python script “frame_sender.py”
8. Start the Training Application on the Hololens[Unity Project name:HMDSmartTrainingAllan-master || Name in Hololens: NotAllanJuxtopiaHoloLens] - keyword to show/hide Heatmap is "show/hide heatmap"
9. Follow the Directions on the screen
10. [Optional] Start the Heatmap Vision Assist to display the heatmap and live gazepoint on the PC using the script 'HeatmapVisionAssist.py' - may crash repeatedly - must be manually restarted multiple times for understanding the utility
11. Once the execution of all application and scripts is complete start the "Analyzer.py" to generate an analysis image
