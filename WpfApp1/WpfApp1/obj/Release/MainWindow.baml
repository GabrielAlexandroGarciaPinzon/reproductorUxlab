   M S B A M L   `   `   `  ���� B  >WpfApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WpfApp1WpfApp1  U QVlc.DotNet.Wpf, Version=2.2.1.0, Culture=neutral, PublicKeyToken=84529da31f4eb963G4clr-namespace:Vlc.DotNet.Wpf;assembly=Vlc.DotNet.WpfVlc.DotNet.Wpf     WpfApp1.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:WpfApp1     Dwpf4clr-namespace:Vlc.DotNet.Wpf;assembly=Vlc.DotNet.Wpf     R NOxyPlot.Wpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf    � Title$  
MainWindow��$
��1024��$
��1280��.��� $��5,5,5,10q� � RowDefinitions �� $��* ��� $	��150 ��� $	��150 ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $��50 ��� $��* ��� $��* ��� $��50 �.+�  Vlc.DotNet.Wpf.VlcControl  -     Name$ 	MyControl��$��0��$��0��$��4��  OxyPlot.Wpf.PlotView  -     Name$ EEGPlot��$��1��$��0��$��2���� -    ��Name$ lineEEG��$y�Hidden=�$��1��$��0��$��2��	 �� X1$ 0��	 �� Y1$ 30��	 �� X2$ 0��		 �� Y2$		 130��
���   �$��4��$��50,0,0,0q�  -   $ 
BrainRPlot��$��1��$��2��$��2���� -   $ 
lineBrainR��$y�Hidden=�$��1��$��2��$��2��$ 0��$ 30��$ 0��$		 130��
���   �$��4��$��50,0,0,0q�  -   $ 
SerialPlot��$��2��$��0��$��4���� -   $ 
lineSerial��$y�Hidden=�$��2��$��0��$��4��$ 0��$ 30��$ 0��$		 130��
���   �$��4��$��50,0,0,0q��� -   
 ��Name$

 Play��$��3��$��0��.��Play�� -	   $
 OpenDirectory��$��4��$��0��.��Open directory�� -
    ��Name$ 
timeSlider��$��3��$��2��$��1��$��3�� � Minimum$ 0Q� � Maximum$ 1Q� � SmallChange$
 0.01Q�$��40��$��0,0,15,0q� � Value �� *	TotalTime+ �� Mode$ TwoWay=�