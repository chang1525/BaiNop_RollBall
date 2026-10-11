# Survival Shooter Lite

Game bắn zombie 3D cho môn Lập trình Game Cơ Bản, làm bằng Unity 6.2 (6000.2.0f1).

## Cách chơi

- **WASD / phím mũi tên:** di chuyển nhân vật.
- **Chuột:** ngắm. **Nhấn chuột trái hoặc giữ Space:** bắn.
- Bấm **START GAME** để bắt đầu; **P** để tạm dừng hoặc tiếp tục.
- Hạ **8 zombie** và nhặt **3 hộp tiếp tế** trong **120 giây** để thắng.
- Chạm vào hộp tiếp tế màu vàng để nhặt. Mỗi hộp hồi 15 máu (tối đa 100) và thêm 5 giây.
- Zombie xuất hiện nhanh hơn khi đã hạ 3 và 6 con. Máu về 0 hoặc hết giờ thì thua.
- **R** hoặc nút **PLAY AGAIN:** chơi lại.

## Mở project

Trong Unity Hub, chọn **Add project from disk** và trỏ tới thư mục này. Mở bằng Unity 6.2 (6000.2.0f1), mở scene `Assets/Scenes/SurvivalShooterLite.unity`, rồi nhấn **Play**.

Trong Hierarchy có sẵn sân chơi, Player, camera, đèn, đạo cụ, ba hộp tiếp tế và các điểm sinh zombie. Hai loại zombie cùng hộp tiếp tế nằm trong `Assets/Prefabs`. Có thể chọn từng object để xem các giá trị và asset được gắn trong Inspector.

## Script chính

- `PlayerController.cs`: đọc phím di chuyển, xoay nhân vật theo chuột và bắn bằng raycast.
- `ZombieController.cs`: cho zombie đi về phía Player, tấn công và xử lý khi bị bắn.
- `SupplyPickup.cs`: cho hộp tiếp tế quay, dùng `OnTriggerEnter` để nhận biết Player chạm vào và cập nhật tiến độ.
- `GameManager.cs`: sinh zombie theo ba đợt, tính máu, thời gian, mục tiêu thắng và hiện giao diện bắt đầu/tạm dừng/kết thúc.

Phần nhặt vật phẩm dùng cách quay và va chạm trigger đã học trong bài **Roll a Ball**. Phần giao diện có trạng thái bắt đầu, tạm dừng và kết quả để người chơi biết mình đang ở đâu trong ván chơi.

## Asset giáo viên

Nhân vật Player, zombie ZomBear và Zombunny, đạo cụ Train và Robot, texture tương ứng, hiệu ứng âm thanh và nhạc nền được lấy từ `Asset Bai Tap/Game3D_Survivor Shooter Asset` trong [repository giáo viên](https://github.com/cditc2025/LapTrinhGameCoBan). Đây là các asset hiện trực tiếp trong gameplay, không chỉ dùng làm tài liệu tham khảo.

Thông tin giấy phép gốc nằm trong `SurvivalShooter_Third-PartyNotice.txt` của bộ asset giáo viên. Project bài nộp này nằm riêng và không thay đổi repository giáo viên.
