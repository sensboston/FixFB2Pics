# FixFB2Pics

**FixFB2Pics** is a command-line utility for cleaning and normalizing `<binary>` image tags in [FictionBook2 (FB2)](https://en.wikipedia.org/wiki/FictionBook) files.

It detects embedded images (GIF, WebP, BMP, TGA, etc.), and automatically converts unsupported formats to JPEG with configurable background handling.

---

## 🛠 Features

- Detects image format via `content-type` or byte signature
- Converts non-JPEG/PNG formats to **JPEG (80% quality)**
- Preserves original `id` attribute
- Updates `content-type` to `image/jpeg`
- Supports **alpha flattening** with:
  - transparent background
  - custom solid color (e.g. `--background=#EEEEEE`)

---

## 💡 Usage

```bash
FixFB2Pics.exe book.fb2 [--background=transparent|#RRGGBB]
```

### Examples

```bash
FixFB2Pics.exe mybook.fb2
FixFB2Pics.exe mybook.fb2 --background=transparent
FixFB2Pics.exe mybook.fb2 --background=#FAFAFA
```

---

## 📦 Download

You can find compiled versions in the [Releases section](../../releases).

---

## 📃 License

[MIT License](https://github.com/sensboston/FixFB2Pics?tab=MIT-1-ov-file#readme)

---

# FixFB2Pics

**FixFB2Pics** — это консольная утилита для очистки и нормализации изображений в тэгах `<binary>` файлов [FictionBook2 (FB2)](https://ru.wikipedia.org/wiki/FictionBook).

Она определяет тип изображения (GIF, WebP, BMP, TGA и др.) и автоматически преобразует неподдерживаемые форматы в JPEG с возможностью настройки фонового цвета.

---

## 🛠 Возможности

- Автоопределение формата по `content-type` или сигнатуре байт
- Конвертация всех форматов, кроме JPEG/PNG, в **JPEG (качество 80%)**
- Сохраняет исходный `id`
- Обновляет `content-type` → `image/jpeg`
- Поддерживает **заливку альфа-канала**:
  - прозрачным фоном
  - или указанным цветом (например, `--background=#EEEEEE`)

---

## 💡 Использование

```bash
FixFB2Pics.exe книга.fb2 [--background=transparent|#RRGGBB]
```

### Примеры

```bash
FixFB2Pics.exe роман.fb2
FixFB2Pics.exe роман.fb2 --background=transparent
FixFB2Pics.exe роман.fb2 --background=#FAFAFA
```

---

## 📦 Скачать

Собранные версии доступны в разделе [Releases](../../releases).

---

## 📃 Лицензия

[MIT License](https://github.com/sensboston/FixFB2Pics?tab=MIT-1-ov-file#readme)
