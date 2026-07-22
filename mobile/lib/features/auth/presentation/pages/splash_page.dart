import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:go_router/go_router.dart';
import '../../../../core/themes/app_theme.dart';

class SplashPage extends StatefulWidget {
  const SplashPage({super.key});

  @override
  State<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends State<SplashPage> with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnim;
  late Animation<double> _opacityAnim;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(vsync: this, duration: const Duration(milliseconds: 1000));
    _scaleAnim = Tween<double>(begin: 0.6, end: 1.0).animate(CurvedAnimation(parent: _controller, curve: Curves.elasticOut));
    _opacityAnim = Tween<double>(begin: 0.0, end: 1.0).animate(CurvedAnimation(parent: _controller, curve: const Interval(0.0, 0.6)));
    _controller.forward();
    _navigateAfterDelay();
  }

  void _navigateAfterDelay() async {
    await Future.delayed(const Duration(milliseconds: 1800));
    if (!mounted) return;

    final storage = const FlutterSecureStorage();
    final token = await storage.read(key: 'access_token');
    if (!mounted) return;

    if (token != null) {
      context.go('/dashboard');
    } else {
      context.go('/login');
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Center(
        child: AnimatedBuilder(
          animation: _controller,
          builder: (context, child) {
            return Opacity(
              opacity: _opacityAnim.value,
              child: Transform.scale(
                scale: _scaleAnim.value,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Container(
                      width: 88,
                      height: 88,
                      decoration: BoxDecoration(
                        gradient: const LinearGradient(
                          colors: [AppColors.primary, AppColors.secondary],
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                        ),
                        borderRadius: BorderRadius.circular(24),
                        boxShadow: [
                          BoxShadow(color: AppColors.primary.withOpacity(0.4), blurRadius: 32, spreadRadius: 4),
                        ],
                      ),
                      child: const Icon(Icons.auto_graph_rounded, color: Colors.white, size: 52),
                    ),
                    const SizedBox(height: 24),
                    const Text(
                      'FundWise AI',
                      style: TextStyle(fontSize: 30, fontWeight: FontWeight.bold, color: AppColors.textPrimary, letterSpacing: -0.5),
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Your finances, on autopilot',
                      style: TextStyle(fontSize: 15, color: AppColors.textSecondary),
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
