import 'package:flutter/material.dart';

class AppColors {
  // Primary Palette
  static const Color background = Color(0xFF0F0E17);
  static const Color surface = Color(0xFF1B1A27);
  static const Color surfaceLight = Color(0xFF28263B);
  
  static const Color primary = Color(0xFF7F56D9);
  static const Color primaryLight = Color(0xFF9E77ED);
  static const Color secondary = Color(0xFF06D6A0);
  
  static const Color cardBg = Color(0x1FFFFFFF);
  static const Color cardBorder = Color(0x2BFFFFFF);

  // Category Colors
  static const Color foodAndDrink = Color(0xFFFF6B6B);
  static const Color groceries = Color(0xFF4ECDC4);
  static const Color transport = Color(0xFFFFD166);
  static const Color bills = Color(0xFF118AB2);
  static const Color shopping = Color(0xFFF72585);
  static const Color entertainment = Color(0xFF7209B7);
  static const Color health = Color(0xFF06D6A0);
  static const Color education = Color(0xFF4CC9F0);
  static const Color transfer = Color(0xFF8338EC);
  static const Color other = Color(0xFF95A5A6);

  // Status Colors
  static const Color success = Color(0xFF06D6A0);
  static const Color warning = Color(0xFFFFD166);
  static const Color error = Color(0xFFFF5252);
  static const Color textPrimary = Color(0xFFFFFFFE);
  static const Color textSecondary = Color(0xFF94A3B8);
  static const Color textMuted = Color(0xFF64748B);
}

class AppTheme {
  static ThemeData get darkTheme {
    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.dark,
      scaffoldBackgroundColor: AppColors.background,
      colorScheme: const ColorScheme.dark(
        primary: AppColors.primary,
        secondary: AppColors.secondary,
        surface: AppColors.surface,
        background: AppColors.background,
        error: AppColors.error,
      ),
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.transparent,
        elevation: 0,
        centerTitle: true,
        titleTextStyle: TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.bold,
          color: AppColors.textPrimary,
        ),
      ),
      cardTheme: CardTheme(
        color: AppColors.surface,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
          side: const BorderSide(color: AppColors.cardBorder, width: 1),
        ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
          elevation: 0,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          textStyle: const TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
    );
  }
}
