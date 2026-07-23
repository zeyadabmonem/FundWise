import 'package:flutter_test/flutter_test.dart';
import 'package:fundwise/main.dart';

void main() {
  testWidgets('App loads smoke test', (WidgetTester tester) async {
    await tester.pumpWidget(const FundWiseApp());
    expect(find.byType(FundWiseApp), findsOneWidget);
  });
}
