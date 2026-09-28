using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain;

namespace DesktopUI.ViewModels
{
    public partial class TrainingViewModel : ViewModelBase
    {
        [ObservableProperty] private bool _isSingleMode = true; 
        [ObservableProperty] private bool _isDoubleMode;
        [ObservableProperty] private bool _isTripleMode;
        [ObservableProperty] private bool _isRandomMode;
        [ObservableProperty] private bool _isCheckoutMode;

        [ObservableProperty] private string _targetDisplay = "0";
        [ObservableProperty] private bool _isTrainingActive;
        
        [ObservableProperty] private int _totalThrows;
        [ObservableProperty] private int _hits;
        [ObservableProperty] private int _misses;

        [RelayCommand]
        private void StartTraining()
        {
            ResetStats();
            IsTrainingActive = true;
            GenerateNextTarget();
        }

        [RelayCommand]
        private void ResetTraining()
        {
            IsTrainingActive = false;
            TargetDisplay = "0";
            ResetStats();
        }

        [RelayCommand]
        private void Hit()
        {
            Hits++;
            TotalThrows++;
            GenerateNextTarget();
        }

        [RelayCommand]
        private void Miss()
        {
            Misses++;
            TotalThrows++;
            GenerateNextTarget();
        }

        private void ResetStats()
        {
            TotalThrows = 0;
            Hits = 0;
            Misses = 0;
        }
        
        private int ShuffleNumber()
        {
            return Random.Shared.Next(1, 21);
        }

        private void GenerateNextTarget()
        {
            if (IsSingleMode) TargetDisplay = ShuffleNumber().ToString();
            else if (IsDoubleMode) TargetDisplay = "D" + ShuffleNumber().ToString();
            else if (IsTripleMode) TargetDisplay = "T" + ShuffleNumber().ToString();
            else if (IsCheckoutMode)
            {
                List<int> prohibited = new List<int>() { 159, 162, 163, 165, 166, 168, 169 };

                int check = Random.Shared.Next(60, 171);
                while (prohibited.Contains(check))
                    check = Random.Shared.Next(60, 171);                   

                TargetDisplay = Checkout.checkout(check);
            }
            else
            {
                int v = Random.Shared.Next(1, 4);
                switch (v)
                {
                    case 1:
                        TargetDisplay = ShuffleNumber().ToString();
                        break;
                    case 2:
                        TargetDisplay = "D" + ShuffleNumber().ToString();
                        break;
                    case 3:
                        TargetDisplay = "T" + ShuffleNumber().ToString();
                        break;
                }
            }
        }
    }
}