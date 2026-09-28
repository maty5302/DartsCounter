namespace Domain
{
    public static class Checkout
    {
        public static string checkout(int score)
        {
            string checkoutscore = "";
            if (score <= 170)
            {
                switch (score)
                {
                    case 170:
                        checkoutscore = "T20 T20 Bull";
                        break;
                    case 169:
                        checkoutscore = "";
                        break;
                    case 168:
                        checkoutscore = "";
                        break;
                    case 167:
                        checkoutscore = "T20 T19 Bull";
                        break;
                    case 166:
                        checkoutscore = "";
                        break;
                    case 165:
                        checkoutscore = "";
                        break;
                    case 164:
                        checkoutscore = "T20 T18 Bull";
                        break;
                    case 163:
                        checkoutscore = "";
                        break;
                    case 162:
                        checkoutscore = "";
                        break;
                    case 161:
                        checkoutscore = "T20 T17 Bull";
                        break;
                    case 160:
                        checkoutscore = "T20 T20 D20";
                        break;
                    case 159:
                        checkoutscore = "";
                        break;
                    case 158:
                        checkoutscore = "T20 T20 D19";
                        break;
                    case 157:
                        checkoutscore = "T20 T19 D20";
                        break;
                    case 156:
                        checkoutscore = "T20 T20 D18";
                        break;
                    case 155:
                        checkoutscore = "T20 T19 D19";
                        break;
                    case 154:
                        checkoutscore = "T20 T18 D20";
                        break;
                    case 153:
                        checkoutscore = "T20 T19 D18";
                        break;
                    case 152:
                        checkoutscore = "T20 T20 D16";
                        break;
                    case 151:
                        checkoutscore = " T20 T17 D20";
                        break;
                    case 150:
                        checkoutscore = "T20 T18 D18";
                        break;
                    case 149:
                        checkoutscore = "T20 T19 D16 ";
                        break;
                    case 148:
                        checkoutscore = "T20 T16 D20";
                        break;
                    case 147:
                        checkoutscore = "T20 T17 D18";
                        break;
                    case 146:
                        checkoutscore = "T20 T18 D16";
                        break;
                    case 145:
                        checkoutscore = "T20 T15 D20";
                        break;
                    case 144:
                        checkoutscore = "T20 T20 D12";
                        break;
                    case 143:
                        checkoutscore = "T20 T17 D16";
                        break;
                    case 142:
                        checkoutscore = "T20 T14 D20";
                        break;
                    case 141:
                        checkoutscore = "T20 T19 D12";
                        break;
                    case 140:
                        checkoutscore = "T20 T16 D16";
                        break;
                    case 139:
                        checkoutscore = "T19 T14 D20";
                        break;
                    case 138:
                        checkoutscore = "T20 T18 D12";
                        break;
                    case 137:
                        checkoutscore = "T19 T16 D16";
                        break;
                    case 136:
                        checkoutscore = "T20 T20 D8";
                        break;
                    case 135:
                        checkoutscore = "T20 T17 D12";
                        break;
                    case 134:
                        checkoutscore = "T20 T14 D16";
                        break;
                    case 133:
                        checkoutscore = "T20 T19 D8";
                        break;
                    case 132:
                        checkoutscore = "T20 T16 D12";
                        break;
                    case 131:
                        checkoutscore = "T20 T13 D16";
                        break;
                    case 130:
                        checkoutscore = "T20 T20 D5";
                        break;
                    case 129:
                        checkoutscore = "T19 T16 D12";
                        break;
                    case 128:
                        checkoutscore = "T18 T14 D16";
                        break;
                    case 127:
                        checkoutscore = "T20 T17 D8";
                        break;
                    case 126:
                        checkoutscore = "T19 T19 D6";
                        break;
                    case 125:
                        checkoutscore = "25 T20 D20";
                        break;
                    case 124:
                        checkoutscore = "T20 T16 D8";
                        break;
                    case 123:
                        checkoutscore = "T19 T16 D9";
                        break;
                    case 122:
                        checkoutscore = "T18 T20 D4";
                        break;
                    case 121:
                        checkoutscore = "T17 T10 D20";
                        break;
                    case 120:
                        checkoutscore = "T20 20 D20";
                        break;
                    case 119:
                        checkoutscore = "T19 T10 D16";
                        break;
                    case 118:
                        checkoutscore = "T20 18 D20";
                        break;
                    case 117:
                        checkoutscore = "T20 17 D20";
                        break;
                    case 116:
                        checkoutscore = "T20 16 D20";
                        break;
                    case 115:
                        checkoutscore = "T20 15 D20";
                        break;
                    case 114:
                        checkoutscore = "T20 14 D20";
                        break;
                    case 113:
                        checkoutscore = "T20 13 D20";
                        break;
                    case 112:
                        checkoutscore = "T20 12 D20";
                        break;
                    case 111:
                        checkoutscore = "T20 19 D16";
                        break;
                    case 110:
                        checkoutscore = "T20 18 D16";
                        break;
                    case 109:
                        checkoutscore = "T19 20 D16";
                        break;
                    case 108:
                        checkoutscore = "T20 16 D16";
                        break;
                    case 107:
                        checkoutscore = "T19 18 D16";
                        break;
                    case 106:
                        checkoutscore = "T20 14 D16";
                        break;
                    case 105:
                        checkoutscore = "T19 16 D16";
                        break;
                    case 104:
                        checkoutscore = "T18 18 D16";
                        break;
                    case 103:
                        checkoutscore = "T20 3 D20";
                        break;
                    case 102:
                        checkoutscore = "T20 10 D16";
                        break;
                    case 101:
                        checkoutscore = "T20 1 D20";
                        break;
                    case 100:
                        checkoutscore = "T20 D20";
                        break;
                    case 99:
                        checkoutscore = "T19 10 D16";
                        break;
                    case 98:
                        checkoutscore = "T20 D19";
                        break;
                    case 97:
                        checkoutscore = "T19 D20";
                        break;
                    case 96:
                        checkoutscore = "T20 D18";
                        break;
                    case 95:
                        checkoutscore = "T19 D19";
                        break;
                    case 94:
                        checkoutscore = "T18 D20";
                        break;
                    case 93:
                        checkoutscore = "T19 D18";
                        break;
                    case 92:
                        checkoutscore = "T20 D16";
                        break;
                    case 91:
                        checkoutscore = "T17 D20";
                        break;
                    case 90:
                        checkoutscore = "T20 D15";
                        break;
                    case 89:
                        checkoutscore = "T19 D16";
                        break;
                    case 88:
                        checkoutscore = "T16 D20";
                        break;
                    case 87:
                        checkoutscore = "T17 D18";
                        break;
                    case 86:
                        checkoutscore = "T18 D16";
                        break;
                    case 85:
                        checkoutscore = "T15 D20";
                        break;
                    case 84:
                        checkoutscore = "T20 D12";
                        break;
                    case 83:
                        checkoutscore = "T17 D16";
                        break;
                    case 82:
                        checkoutscore = "T14 D20";
                        break;
                    case 81:
                        checkoutscore = "T19 D12";
                        break;
                    case 80:
                        checkoutscore = "T20 D10";
                        break;
                    case 79:
                        checkoutscore = "T19 D11";
                        break;
                    case 78:
                        checkoutscore = "T18 D12";
                        break;
                    case 77:
                        checkoutscore = "T19 D10";
                        break;
                    case 76:
                        checkoutscore = "T20 D8";
                        break;
                    case 75:
                        checkoutscore = "T17 D12";
                        break;
                    case 74:
                        checkoutscore = "T14 D16";
                        break;
                    case 73:
                        checkoutscore = "T19 D8";
                        break;
                    case 72:
                        checkoutscore = "T16 D12";
                        break;
                    case 71:
                        checkoutscore = "T13 D16";
                        break;
                    case 70:
                        checkoutscore = "T10 D20";
                        break;
                    case 69:
                        checkoutscore = "T15 D12";
                        break;
                    case 68:
                        checkoutscore = "T20 D4";
                        break;
                    case 67:
                        checkoutscore = "T17 D8";
                        break;
                    case 66:
                        checkoutscore = "T10 D18";
                        break;
                    case 65:
                        checkoutscore = "T19 D4";
                        break;
                    case 64:
                        checkoutscore = "T16 D8";
                        break;
                    case 63:
                        checkoutscore = "T13 D12";
                        break;
                    case 62:
                        checkoutscore = "T10 D16";
                        break;
                    case 61:
                        checkoutscore = "T15 D8";
                        break;
                    case 60:
                        checkoutscore = "20 D20";
                        break;
                    case 59:
                        checkoutscore = "19 D20";
                        break;
                    case 58:
                        checkoutscore = "18 D20";
                        break;
                    case 57:
                        checkoutscore = "17 D20";
                        break;
                    case 56:
                        checkoutscore = "16 D20";
                        break;
                    case 55:
                        checkoutscore = "15 D20";
                        break;
                    case 54:
                        checkoutscore = "14 D20";
                        break;
                    case 53:
                        checkoutscore = "13 D20";
                        break;
                    case 52:
                        checkoutscore = "12 D20";
                        break;
                    case 51:
                        checkoutscore = "19 D16";
                        break;
                    case 50:
                        checkoutscore = "10 D20";
                        break;
                    case 49:
                        checkoutscore = "17 D16";
                        break;
                    case 48:
                        checkoutscore = "16 D16";
                        break;
                    case 47:
                        checkoutscore = "15 D16";
                        break;
                    case 46:
                        checkoutscore = "6 D20";
                        break;
                    case 45:
                        checkoutscore = "13 D16";
                        break;
                    case 44:
                        checkoutscore = "12 D16";
                        break;
                    case 43:
                        checkoutscore = "3 D20";
                        break;
                    case 42:
                        checkoutscore = "10 D16";
                        break;
                    case 41:
                        checkoutscore = "9 D16";
                        break;
                    case 40:
                        checkoutscore = "D20";
                        break;
                    case 39:
                        checkoutscore = "7 D16";
                        break;
                    case 38:
                        checkoutscore = "D19";
                        break;
                    case 37:
                        checkoutscore = "5 D16";
                        break;
                    case 36:
                        checkoutscore = "D18";
                        break;
                    case 35:
                        checkoutscore = "3 D16";
                        break;
                    case 34:
                        checkoutscore = "D17";
                        break;
                    case 33:
                        checkoutscore = "17 D8";
                        break;
                    case 32:
                        checkoutscore = "D16";
                        break;
                    case 31:
                        checkoutscore = "7 D12";
                        break;
                    case 30:
                        checkoutscore = "D15";
                        break;
                    case 29:
                        checkoutscore = "5 D12";
                        break;
                    case 28:
                        checkoutscore = "D14";
                        break;
                    case 27:
                        checkoutscore = "19 D4";
                        break;
                    case 26:
                        checkoutscore = "D13";
                        break;
                    case 25:
                        checkoutscore = "9 D8";
                        break;
                    case 24:
                        checkoutscore = "D12";
                        break;
                    case 23:
                        checkoutscore = "7 D8";
                        break;
                    case 22:
                        checkoutscore = "D11";
                        break;
                    case 21:
                        checkoutscore = "5 D8";
                        break;
                    case 20:
                        checkoutscore = "D10";
                        break;
                    case 19:
                        checkoutscore = "3 D8";
                        break;
                    case 18:
                        checkoutscore = "D9";
                        break;
                    case 17:
                        checkoutscore = "9 D4";
                        break;
                    case 16:
                        checkoutscore = "D8";
                        break;
                    case 15:
                        checkoutscore = "7 D4";
                        break;
                    case 14:
                        checkoutscore = "D7";
                        break;
                    case 13:
                        checkoutscore = "5 D4";
                        break;
                    case 12:
                        checkoutscore = "D6";
                        break;
                    case 11:
                        checkoutscore = "3 D4";
                        break;
                    case 10:
                        checkoutscore = "D5";
                        break;
                    case 9:
                        checkoutscore = "1 D4";
                        break;
                    case 8:
                        checkoutscore = "D4";
                        break;
                    case 7:
                        checkoutscore = "1 D3";
                        break;
                    case 6:
                        checkoutscore = "D3";
                        break;
                    case 5:
                        checkoutscore = "1 D2";
                        break;
                    case 4:
                        checkoutscore = "D2";
                        break;
                    case 3:
                        checkoutscore = "1 D1";
                        break;
                    case 2:
                        checkoutscore = "D1";
                        break;
                    default:
                        break;
                }
            }
            return checkoutscore;

        }

    }
}
